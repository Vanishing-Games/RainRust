# Audio System — 编码规范

## 架构概述

音频系统基于事件驱动，由三层构成：

```
MessageBroker (全局事件总线)
    │
    ▼
AudioEventSheet (配置表 ScriptableObject)
    │  持有多个 AudioEntry
    ▼
VgAudioManager (MonoSingletonPersistent)
    │  Awake 时读取所有 Sheet，注册订阅
    ▼
FMOD Runtime API
```

`VgAudioManager` 在 `Awake` 阶段扫描所有 `AudioEventSheet`，为每个 `AudioEntry` 通过反射订阅 `ListenEventType` 对应的事件。事件触发时调用 `entry.Execute()`，由 Entry 自行决定如何驱动 FMOD。

---

## 核心类型

### AudioEntry（抽象基类）

```
AudioEntry
├── ListenEventType   订阅的事件类型（Inspector 下拉选择）
├── TriggerMode       OnNext / OnError / OnComplete
└── Execute(IEvent, VgAudioManager)   必须实现
```

**直接继承 `AudioEntry` 的场景**：Entry 需要从事件对象本身获取播放所需的全部信息（FMOD event、位置、参数等），无需静态配置 EventReference。

### AudioPlayEntry : AudioEntry

在 `AudioEntry` 基础上增加：

```
AudioPlayEntry
├── FmodEvent         静态配置的 FMOD EventReference
├── PlayMode          OneShot / Managed
└── Managed           ManagedConfig（仅 PlayMode == Managed 时使用）
```

**继承 `AudioPlayEntry` 的场景**：FMOD 事件在 Inspector 中静态配置，或需要 Managed 模式的生命周期管理（含 Stop 订阅自动注册）。

### AudioParameterEntry : AudioEntry

用于向已有 Managed 实例设置 FMOD 参数，不触发播放/停止。

```
AudioParameterEntry
├── ManagedId         目标 Managed 实例的 ID
└── ResolveParameters(IEvent)   子类实现，返回要设置的参数列表
```

---

## Managed 模式规范

Managed 模式允许对一个持续播放的 FMOD 实例进行追踪与控制（Stop、参数更新等）。

### ManagedConfig 字段

| 字段 | 说明 |
|------|------|
| `Id` | 实例唯一标识符，**必须全局唯一**，建议用语义名如 `"bgm"`、`"ambience"` |
| `StopEventType` | 触发停止的事件类型 |
| `StopMode` | `ALLOWFADEOUT`（默认）或 `IMMEDIATE` |
| `RestartIfPlaying` | 已在播放时是否重启 |

### Stop 订阅的自动注册条件

`VgAudioManager.RegisterAllEntries` 在以下条件**同时满足**时，自动为 Entry 注册 Stop 事件订阅：

1. Entry 是 `AudioPlayEntry` 的子类
2. `PlayMode == AudioPlayMode.Managed`
3. `Managed.StopEventType != null`
4. `Managed.StopEventType != ListenEventType`

**凡是使用 Managed 播放的 Entry，必须继承 `AudioPlayEntry` 而非 `AudioEntry`**，否则 Stop 订阅不会被注册。

---

## Entry 类型一览

| 类型 | 基类 | 事件接口要求 | 说明 |
|------|------|-------------|------|
| `DefaultAudioEntry` | `AudioPlayEntry` | 无 | OneShot 或 Managed，EventReference 静态配置 |
| `Audio3DAudioEntry` | `AudioPlayEntry` | `IPositionEvent` | 带 3D 位置的播放，从事件读取坐标 |
| `StringManagedAudioEntry` | `AudioPlayEntry` | `IStringValueEvent` | 动态拼接事件路径，Managed 模式，路径 = `EventPathPrefix + Value` |
| `DirectPlayOneShotEntry` | `AudioEntry` | `IFmodOneShotEvent` | 事件携带 EventReference，直接 OneShot |
| `DirectPlay3DOneShotEntry` | `AudioEntry` | `IFmodPositionEvent` | 事件携带 EventReference + 位置，3D OneShot |
| `DirectPlayManagedEntry` | `AudioEntry` | `IFmodOneShotEvent` | 事件携带 EventReference，Managed 播放 |
| `DirectSetParameterEntry` | `AudioEntry` | `IFmodParameterEvent` | 事件携带参数名/值，设置到指定 Managed 实例 |
| `SetParameterEntry` | `AudioParameterEntry` | 无 | Inspector 静态配置参数列表，设置到 ManagedId 实例 |
| `SetParameterFromEventEntry` | `AudioParameterEntry` | `IFloatValueEvent` | 从事件读取 float 值，设置到 ManagedId 实例 |
| `CustomAudioEntry`（抽象） | `AudioPlayEntry` | 自定义 | 重写 `ResolveParameters` 和 `ResolvePosition` |
| `CustomParameterUpdateEntry`（抽象） | `AudioParameterEntry` | 自定义 | 重写 `ResolveManagedId` 动态决定目标实例 |

---

## 事件接口规范

Entry 所需的事件接口均定义在 `Core.CoreModule.Audio.Interfaces`：

| 接口 | 字段 | 用途 |
|------|------|------|
| `IStringValueEvent` | `string Value` | 携带字符串，用于动态路径拼接等 |
| `IFloatValueEvent` | `float Value` | 携带浮点数，用于参数更新 |
| `IPositionEvent` | `Vector3 Position` | 携带世界坐标，用于 3D 音效 |
| `IFmodOneShotEvent` | `EventReference FmodEvent` | 携带 EventReference，Direct 系列 Entry 使用 |
| `IFmodPositionEvent` | `EventReference FmodEvent` + `Vector3 Position` | 携带 EventReference + 坐标 |
| `IFmodParameterEvent` | `string ManagedId` + `string ParameterName` + `float Value` | 携带完整参数信息 |

---

## 如何新增一个 Entry

### 场景 A：静态 EventReference + Managed 播放

```csharp
[Serializable]
public class MyManagedEntry : AudioPlayEntry
{
    public override void Execute(IEvent e, VgAudioManager manager)
    {
        manager.PlayManaged(Managed.Id, FmodEvent, Managed);
    }
}
```

在 Inspector 中配置：`PlayMode = Managed`，`Managed.Id`，`Managed.StopEventType`。

### 场景 B：事件携带全部信息（Direct 风格）

```csharp
[Serializable]
public class MyDirectEntry : AudioEntry
{
    public override void Execute(IEvent e, VgAudioManager manager)
    {
        if (e is not IFmodOneShotEvent oneShotEvent) return;
        RuntimeManager.PlayOneShot(oneShotEvent.FmodEvent);
    }
}
```

**注意**：直接继承 `AudioEntry` 的 Entry 不支持 Stop 订阅自动注册。如果需要 Managed 生命周期，必须改用 `AudioPlayEntry`。

### 场景 C：动态事件路径 + Managed 播放

```csharp
[Serializable]
public class MyDynamicEntry : AudioPlayEntry
{
    public string EventPathPrefix = "event:/SFX/";

    public MyDynamicEntry() { PlayMode = AudioPlayMode.Managed; }

    public override void Execute(IEvent e, VgAudioManager manager)
    {
        if (e is not IStringValueEvent stringEvent) return;
        var eventRef = EventReference.Find(EventPathPrefix + stringEvent.Value);
        manager.PlayManaged(Managed.Id, eventRef, Managed);
    }
}
```

`EventReference.Find` 要求 FMOD EventManager 已初始化。若调用时机早于初始化，参考 `StringManagedAudioEntry` 的 UniTask retry 模式。

---

## 配置流程

1. 在 `Project` 窗口 右键 → `Create/Core/Audio/AudioEventSheet` 创建配置表
2. 在表中添加 Entry，配置 `ListenEventType` 和播放参数
3. 将 Sheet 拖入场景中 `VgAudioManager` 的 `Sheets` 列表
4. 运行时 `VgAudioManager.Awake` 自动完成订阅注册

---

## 常见错误

**Stop 事件不触发**
- Entry 没有继承 `AudioPlayEntry`，Stop 订阅未被注册
- `PlayMode` 未设为 `Managed`
- `Managed.Id` 与播放时使用的 Id 不一致

**`EventReference.Find` 抛出 `InvalidOperationException`**
- FMOD EventManager 尚未初始化（通常发生在游戏启动极早期）
- 在 Execute 内部使用 UniTask retry 循环等待初始化完成（见 `StringManagedAudioEntry`）

**同一 Id 的音频被意外覆盖**
- `Managed.Id` 重复，检查所有 Sheet 中的 Managed Id 是否全局唯一
