<!--
 * --------------------------------------------------------------------------------
 * Copyright (c) 2025 Vanishing Games. All Rights Reserved.
 * @Author: VanishXiao
 * @Date: 2026-04-09 20:00:28
 * @LastEditTime: 2026-04-09 20:31:20
 * --------------------------------------------------------------------------------
-->

# Audio System Design

基于 FMOD + R3 事件驱动的音频系统设计文档。

---

## 系统架构总览

```
AudioEventSheet (SO)
  └── List<AudioEntry>
        ├── DefaultAudioEntry          — 忽略事件载荷，直接触发
        ├── Audio3DAudioEntry          — 从事件提取 Position 传给 FMOD 3D API
        ├── CustomAudioEntry           — 抽象基类，子类实现参数映射逻辑
        ├── SetParameterEntry          — 固定参数值写入 Managed 实例
        ├── SetParameterFromEventEntry — 从 IFloatValueEvent 提取单值映射到参数
        └── CustomParameterUpdateEntry — 抽象基类，子类实现参数提取逻辑

VgAudioManager (MonoSingletonPersistent)
  ├── AudioEventSheet[]           — Inspector 中配置多张表
  ├── 运行时订阅 MessageBroker    — 根据每条 Entry 的描述动态订阅
  └── Dictionary<string, EventInstance>  — Managed 实例注册表
```

数据流：
```
游戏逻辑  →  MessageBroker.Publish<TEvent>()
          →  VgAudioManager 订阅回调
          →  AudioEntry 解析 + 分发
          →  FMOD RuntimeManager / EventInstance
```

---

## 核心类型

### AudioEventSheet

```csharp
// ScriptableObject
// 路径建议: Assets/GameMain/Audio/Sheets/
[CreateAssetMenu(fileName = "AudioEventSheet", menuName = "VG/Audio/AudioEventSheet")]
public class AudioEventSheet : ScriptableObject
{
    [ListDrawerSettings(ShowFoldout = true)]
    public List<AudioEntry> Entries;
}
```

### AudioEntry（抽象基类）

```csharp
[Serializable]
public abstract class AudioEntry
{
    // 监听的事件类型（运行时通过反射订阅）
    // Odin: TypeFilter 限制为 IEvent 子类，存全限定名
    [ValidateInput(nameof(ValidateEventType))]
    public SerializedType ListenEventType;

    // 触发时机：OnNext / OnComplete / OnError
    public TriggerMode TriggerMode = TriggerMode.OnNext;

    // 播放模式
    public AudioPlayMode PlayMode = AudioPlayMode.OneShot;

    // OneShot：播完即释放
    // Managed：需要额外配置 ManagedConfig

    [ShowIf(nameof(PlayMode), AudioPlayMode.Managed)]
    public ManagedConfig Managed;

    public abstract void Execute(IEvent e, VgAudioManager manager);
}
```

---

## TriggerMode

| 值 | 对应 MessageBroker 订阅参数 | 含义 |
|----|----------------------------|------|
| `OnNext` | `onNext` handler | 事件每次发布时触发 |
| `OnComplete` | `onCompleted` handler | 流结束（正常）时触发 |
| `OnError` | `onError` handler | 流异常时触发 |

`VgAudioManager` 在注册时根据 `TriggerMode` 分别填入对应 lambda：

```csharp
// 伪代码示意
MessageBroker.Global.Subscribe<TEvent>(
    onNext:      e => entry.TriggerMode == OnNext      ? entry.Execute(e, this) : null,
    onError:     ex => entry.TriggerMode == OnError    ? entry.Execute(null, this) : null,
    onCompleted: () => entry.TriggerMode == OnComplete ? entry.Execute(null, this) : null
);
```

由于 `ListenEventType` 在运行时才确定，订阅需要通过反射泛型调用：

```csharp
// 反射构造泛型订阅
private void SubscribeEntry(AudioEntry entry)
{
    var eventType = entry.ListenEventType.Type;
    var method = typeof(VgAudioManager)
        .GetMethod(nameof(SubscribeEntryGeneric), BindingFlags.NonPublic | BindingFlags.Instance)
        .MakeGenericMethod(eventType);
    var disposable = (IDisposable)method.Invoke(this, new object[] { entry });
    m_Subscriptions.Add(disposable);
}

private IDisposable SubscribeEntryGeneric<TEvent>(AudioEntry entry) where TEvent : IEvent
{
    return MessageBroker.Global.Subscribe<TEvent>(
        onNext: e => { if (entry.TriggerMode == TriggerMode.OnNext) entry.Execute(e, this); },
        onError: ex => { if (entry.TriggerMode == TriggerMode.OnError) entry.Execute(null, this); },
        onCompleted: () => { if (entry.TriggerMode == TriggerMode.OnComplete) entry.Execute(null, this); }
    );
}
```

---

## AudioEntry 类型继承体系

### DefaultAudioEntry

不关心事件载荷，直接触发 FMOD 事件。

```csharp
[Serializable]
public class DefaultAudioEntry : AudioEntry
{
    public EventReference FmodEvent;

    public override void Execute(IEvent e, VgAudioManager manager)
    {
        if (PlayMode == AudioPlayMode.OneShot)
            RuntimeManager.PlayOneShot(FmodEvent);
        else
            manager.PlayManaged(Managed.Id, FmodEvent, Managed);
    }
}
```

### Audio3DAudioEntry

要求监听的事件实现 `IPositionEvent` 接口，从中提取世界坐标传给 FMOD。

```csharp
// 约定接口，3D 音效事件需实现
public interface IPositionEvent : IEvent
{
    Vector3 Position { get; }
}

[Serializable]
public class Audio3DAudioEntry : AudioEntry
{
    public EventReference FmodEvent;

    public override void Execute(IEvent e, VgAudioManager manager)
    {
        if (e is not IPositionEvent posEvent)
        {
            CLogger.LogWarning(
                $"Audio3DAudioEntry: event {e?.GetType().Name} does not implement IPositionEvent",
                LogTag.Audio
            );
            return;
        }

        if (PlayMode == AudioPlayMode.OneShot)
            RuntimeManager.PlayOneShot(FmodEvent, posEvent.Position);
        else
            manager.PlayManaged3D(Managed.Id, FmodEvent, posEvent.Position, Managed);
    }
}
```

### CustomAudioEntry（抽象基类）

处理「事件类型与 FMOD 所需参数不直接对应」的场景，例如：
- `PlayerDeathEvent` 携带 `Player` 引用，但 FMOD 需要 `deathCount` 参数
- `EnemyAttackEvent` 携带 `Damage` 值，需映射为 FMOD 的 `Intensity` 标签

```csharp
[Serializable]
public abstract class CustomAudioEntry : AudioEntry
{
    public EventReference FmodEvent;

    // 子类重写：从事件中提取 FMOD 参数列表
    // 返回 null 或空数组表示无额外参数
    protected abstract FmodParameterPair[] ResolveParameters(IEvent e);

    // 子类可重写：从事件中提取播放位置（3D 音效）
    // 返回 null 表示 2D 播放
    protected virtual Vector3? ResolvePosition(IEvent e) => null;

    public override void Execute(IEvent e, VgAudioManager manager)
    {
        var parameters = ResolveParameters(e);
        var position = ResolvePosition(e);

        if (PlayMode == AudioPlayMode.OneShot)
        {
            var instance = RuntimeManager.CreateInstance(FmodEvent);
            ApplyParameters(instance, parameters);

            if (position.HasValue)
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

            instance.start();
            instance.release();
        }
        else
        {
            manager.PlayManagedCustom(Managed.Id, FmodEvent, parameters, position, Managed);
        }
    }

    private static void ApplyParameters(FMOD.Studio.EventInstance instance, FmodParameterPair[] parameters)
    {
        if (parameters == null) return;
        foreach (var p in parameters)
            instance.setParameterByName(p.Name, p.Value);
    }
}

[Serializable]
public struct FmodParameterPair
{
    public string Name;
    public float Value;
}
```

**使用示例 — PlayerDeathEntry:**

```csharp
[Serializable]
public class PlayerDeathEntry : CustomAudioEntry
{
    protected override FmodParameterPair[] ResolveParameters(IEvent e)
    {
        if (e is PlayerDeathEvent deathEvent)
            return new[] { new FmodParameterPair { Name = "DeathCount", Value = deathEvent.DeathCount } };
        return null;
    }

    protected override Vector3? ResolvePosition(IEvent e)
        => e is PlayerDeathEvent d ? d.Player.transform.position : null;
}
```

---

## AudioPlayMode

```csharp
public enum AudioPlayMode
{
    // 播放一次即释放，RuntimeManager.PlayOneShot 或 CreateInstance + start + release
    OneShot,

    // 持续型实例（BGM、环境音），由 VgAudioManager 维护生命周期
    Managed,
}
```

---

## ManagedConfig

```csharp
[Serializable]
public class ManagedConfig
{
    // Managed 实例唯一 ID，用于查找/停止
    public string Id;

    // 触发停止的事件类型
    public SerializedType StopEventType;

    // 停止时的淡出模式
    public FMOD.Studio.STOP_MODE StopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

    // 保证唯一：若同 ID 实例已存在，是否先停止再重建
    public bool RestartIfPlaying = false;
}
```

---

## VgAudioManager

### 职责

1. 持有 `AudioEventSheet[]` 配置
2. Awake 时根据所有 Entry 动态订阅 MessageBroker
3. 维护 `Dictionary<string, FMOD.Studio.EventInstance>` Managed 实例注册表
4. 提供 `PlayManaged` / `StopManaged` 等辅助 API
5. OnDestroy 时释放所有订阅 + 停止所有 Managed 实例

### 结构草图

```csharp
namespace Core
{
    public class VgAudioManager : MonoSingletonPersistent<VgAudioManager>
    {
        // ── 公开 API ─────────────────────────────────────────────────

        public void PlayManaged(string id, EventReference fmodEvent, ManagedConfig config) { … }
        public void PlayManaged3D(string id, EventReference fmodEvent, Vector3 position, ManagedConfig config) { … }
        public void PlayManagedCustom(string id, EventReference fmodEvent, FmodParameterPair[] parameters, Vector3? position, ManagedConfig config) { … }
        public void SetManagedParameter(string id, FmodParameterPair[] parameters) { … }
        public void StopManaged(string id, FMOD.Studio.STOP_MODE stopMode) { … }
        public bool IsManagedPlaying(string id) { … }

        // ── 初始化 ───────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            LoadAllBanks();
            RegisterAllEntries();
        }

        private void OnDestroy()
        {
            UnregisterAll();
            StopAllManaged();
        }

        // ── Inspector 配置 ───────────────────────────────────────────

        [SerializeField]
        [ListDrawerSettings(ShowFoldout = true)]
        private AudioEventSheet[] m_Sheets;

        // ── 字段 ─────────────────────────────────────────────────────

        private List<IDisposable> m_Subscriptions = new();
        private Dictionary<string, FMOD.Studio.EventInstance> m_ManagedInstances = new();
    }
}
```

### Managed 实例唯一性保证

```
PlayManaged(id, …)
  ├── id 不存在 → 创建新实例，注册到字典
  └── id 已存在
        ├── RestartIfPlaying=true  → Stop(旧) + Create(新)
        └── RestartIfPlaying=false → 忽略，直接返回
```

---

## 加载与内存管理

当前阶段：项目规模较小，启动时一次性加载全部 Bank。

```csharp
private void LoadAllBanks()
{
    // FMOD 在 Unity 中通过 FMODUnity.Settings 自动加载 Master Bank
    // 其余 Bank 按需在此补充
    // 示例：
    // RuntimeManager.LoadBank("Music", loadSamples: true);
    // RuntimeManager.LoadBank("SFX", loadSamples: true);

    CLogger.LogInfo("All FMOD Banks loaded", LogTag.Audio);
}
```

未来扩展方向（暂不实现）：
- 按关卡/场景异步加载 Bank（对接现有 `LoadRequestEvent` 体系）
- Bank 引用计数，场景卸载时释放

---

## FMOD 参数支持

Managed 实例在运行期可能需要实时修改 FMOD 参数（如 BGM Intensity 随关卡进度变化）。
参数更新通过专用的 `AudioParameterEntry` 子类配置在 `AudioEventSheet` 中，策划无需编写任何代码即可完成常见场景的配置。

### AudioParameterEntry（抽象基类）

继承 `AudioEntry`，隐藏播放相关字段，增加 `ManagedId` 指向目标 Managed 实例。

```csharp
[Serializable]
public abstract class AudioParameterEntry : AudioEntry
{
    [BoxGroup("Parameter")]
    public string ManagedId;

    // 子类实现：从事件中解析需要写入的参数列表
    protected abstract FmodParameterPair[] ResolveParameters(IEvent e);

    public sealed override void Execute(IEvent e, VgAudioManager manager)
    {
        var parameters = ResolveParameters(e);
        if (parameters == null || parameters.Length == 0)
            return;
        manager.SetManagedParameter(ManagedId, parameters);
    }
}
```

> `PlayMode` / `Managed` 字段对参数更新无意义，用 `[HideInInspector]` 在子类中隐藏。

---

### SetParameterEntry

最常用类型。Inspector 直接配置固定参数名与值，事件触发时写入。

```csharp
[Serializable]
public class SetParameterEntry : AudioParameterEntry
{
    [BoxGroup("Parameter")]
    public FmodParameterPair[] Parameters;

    protected override FmodParameterPair[] ResolveParameters(IEvent e) => Parameters;
}
```

**Inspector 配置示例 — 过场动画开始时降低 BGM 音量：**

```
ListenEventType = CutsceneStartEvent
TriggerMode     = OnNext
ManagedId       = "bgm_main"
Parameters      = [{ Name = "Volume", Value = 0.3 }]
```

---

### SetParameterFromEventEntry

适用于参数值来自事件载荷的场景（如「当前生命值影响 BGM 紧张度」）。
要求监听的事件实现 `IFloatValueEvent` 接口，将其 `Value` 映射到指定 FMOD 参数。

```csharp
// 约定接口：携带单个 float 值的事件
public interface IFloatValueEvent : IEvent
{
    float Value { get; }
}

[Serializable]
public class SetParameterFromEventEntry : AudioParameterEntry
{
    [BoxGroup("Parameter")]
    public string ParameterName;

    protected override FmodParameterPair[] ResolveParameters(IEvent e)
    {
        if (e is not IFloatValueEvent floatEvent)
        {
            CLogger.LogWarning(
                $"SetParameterFromEventEntry: event {e?.GetType().Name} does not implement IFloatValueEvent",
                LogTag.Audio
            );
            return null;
        }

        return new[] { new FmodParameterPair { Name = ParameterName, Value = floatEvent.Value } };
    }
}
```

**使用示例 — PlayerHealthChangedEvent 驱动 BGM 紧张度：**

```csharp
// 游戏逻辑层只需实现接口，无需关心音频路由
public class PlayerHealthChangedEvent : IFloatValueEvent
{
    public float Value { get; }
    public PlayerHealthChangedEvent(float normalizedHp) => Value = normalizedHp;
}
```

```
Inspector 配置:
  ListenEventType = PlayerHealthChangedEvent
  TriggerMode     = OnNext
  ManagedId       = "bgm_level01"
  ParameterName   = "Intensity"
```

---

### CustomParameterUpdateEntry（抽象基类）

处理「需要从事件中提取多个参数，或参数值需要非线性计算」的场景。
子类重写 `ResolveParameters`，也可重写 `ResolveManagedId` 使目标实例动态化。

```csharp
[Serializable]
public abstract class CustomParameterUpdateEntry : AudioParameterEntry
{
    // 子类可重写以动态决定目标实例（默认使用 Inspector 中配置的 ManagedId）
    protected virtual string ResolveManagedId(IEvent e) => ManagedId;

    public sealed override void Execute(IEvent e, VgAudioManager manager)
    {
        var id = ResolveManagedId(e);
        var parameters = ResolveParameters(e);
        if (parameters == null || parameters.Length == 0)
            return;
        manager.SetManagedParameter(id, parameters);
    }
}
```

**使用示例 — EnemyCountChangedEvent 同时更新多个参数：**

```csharp
[Serializable]
public class EnemyCountParameterEntry : CustomParameterUpdateEntry
{
    protected override FmodParameterPair[] ResolveParameters(IEvent e)
    {
        if (e is not EnemyCountChangedEvent ev)
            return null;

        return new[]
        {
            new FmodParameterPair { Name = "EnemyCount",  Value = ev.Count },
            new FmodParameterPair { Name = "ThreatLevel", Value = Mathf.Log(ev.Count + 1) },
        };
    }
}
```

---

### VgAudioManager — SetManagedParameter

```csharp
public void SetManagedParameter(string id, FmodParameterPair[] parameters)
{
    if (!m_ManagedInstances.TryGetValue(id, out var instance))
    {
        CLogger.LogWarning($"SetManagedParameter: Managed instance '{id}' not found", LogTag.Audio);
        return;
    }

    foreach (var p in parameters)
        instance.setParameterByName(p.Name, p.Value);
}

---

## Odin Inspector 配置设计

目标：在 Inspector 中直接配置 AudioEventSheet，体验等同于「可视化路由表」。

### SerializedType 事件类型选择器

借助 Odin 的 `[TypeFilter]`，限制选择范围为 `IEvent` 所有子类：

```csharp
[TypeFilter(nameof(GetEventTypes))]
public SerializedType ListenEventType;

private static IEnumerable<Type> GetEventTypes()
    => AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
```

### 分层折叠与条件显示

```csharp
[BoxGroup("Trigger")]
public TriggerMode TriggerMode;

[BoxGroup("Playback")]
public AudioPlayMode PlayMode;

[BoxGroup("Playback/Managed Config")]
[ShowIf(nameof(PlayMode), AudioPlayMode.Managed)]
public ManagedConfig Managed;
```

### Entry 多态列表

`AudioEventSheet.Entries` 使用 `List<AudioEntry>`（抽象基类），Odin 自动处理多态序列化：

```csharp
[SerializeField]
[ListDrawerSettings(
    ShowFoldout = true,
    CustomAddFunction = nameof(AddEntry),
    HideAddButton = false
)]
private List<AudioEntry> m_Entries;
```

通过 `OdinMenuEditorWindow` 或下拉菜单，让用户选择添加哪种 Entry 子类型。

---

## 日志标签

新增 `LogTag.Audio`（需在 `LogTag` 枚举中添加），所有 VgAudioManager 相关日志使用此 Tag：

```csharp
CLogger.LogInfo($"Playing OneShot: {fmodEvent.Path}", LogTag.Audio);
CLogger.LogWarning($"Managed instance '{id}' not found", LogTag.Audio);
CLogger.LogError($"3D entry event does not implement IPositionEvent", LogTag.Audio);
```

---

## 与现有系统集成

### 关卡切换 BGM

```
LevelManager.LoadRoom()
  → MessageBroker.Publish(new LevelSwitchEvent(...))
  → AudioEventSheet 中配置:
      ListenEvent = LevelSwitchEvent
      TriggerMode = OnNext
      PlayMode    = Managed
      Managed.Id  = "bgm_main"
      Managed.StopEventType = LevelSwitchEvent  (再次切换时停止旧 BGM)
      Managed.RestartIfPlaying = true
```

### 玩家死亡音效（3D）

```
PlayerController
  → MessageBroker.Publish(new PlayerDeathEvent(player))
  → Audio3DAudioEntry:
      ListenEvent = PlayerDeathEvent  (需实现 IPositionEvent)
      TriggerMode = OnNext
      PlayMode    = OneShot
      FmodEvent   = event:/SFX/Player/Death
```

---

## 待定与遗留问题

| 问题 | 现状 | 建议 |
|------|------|------|
| `LogTag.Audio` 是否已存在 | 待确认 | 在 `LogTag` 枚举中补充 |
| `SerializedType` Odin 版本兼容性 | 待验证 | 若不支持多态列表，改为 `[OdinSerialize]` + `List<object>` |
| Bank 加载策略 | 目前全量加载 | 后续对接 `LoadRequestEvent` 体系做按需加载 |
| `AudioParameterEntry` 隐藏 `PlayMode`/`Managed` 字段 | 待验证 | 确认 Odin `[HideInInspector]` 对继承字段的生效范围 |
| `SetParameterFromEventEntry` 多值支持 | 当前仅支持单 float | 若需多值可扩展为 `IMultiValueEvent` 或改用 `CustomParameterUpdateEntry` |
| StopManaged 订阅时机 | 在 `PlayManaged` 时动态注册 Stop 订阅 | 或在 Awake 全量注册，需权衡 |
