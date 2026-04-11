# GameCore 重构设计文档

> 版本：2.0  
> 日期：2026-04-11  
> 状态：设计确认阶段

---

## 一、核心思路

### 通信模型三原则

系统间的通信方式是整个架构的地基，必须首先明确：

```
┌──────────────────────────────────────────────────────────────────┐
│                        GameCore                                  │
│                    （唯一的流程决策者）                          │
│                                                                  │
│  → 通过 Hook 驱动系统（自上而下）                                │
│  ← 通过订阅特定事件感知系统（自下而上，但 GameCore 主动选择订阅）│
└────────────┬────────────────────────────────┬────────────── ─────┘
     Hook 驱动↓                     订阅事件↑ │
┌────────────▼────────────┐   ┌───────────────▼──────────┐
│    ICoreModuleSystem    │   │    ICoreModuleSystem     │
│    VgAudioSystem        │   │    LevelManager          │
│    VgSceneManager       │◄──►    PlayerManageSystem    │
│    VgSaveSystem  ...    │   │    ...                   │
└─────────────────────────┘   └──────────────────────────┘
         ↑↓ MessageBroker 事件（系统间平等广播，无层级关系）
```

| 通信方向 | 机制 | 原因 |
|---|---|---|
| GameCore → 系统 | Hook（系统注册，GameCore 驱动） | GameCore 是流程驱动者，主动推动 |
| 系统 → GameCore | 特定事件（GameCore 主动订阅） | GameCore 观察，但不被控制 |
| 系统 ↔ 系统 | MessageBroker 事件广播 | 平等解耦，不依赖层级 |

### 架构全图

```
GameCore
├── FSM（UnityHFSM）
│   None → Booting → MainMenu ↔ Loading ↔ InLevel
│
├── CoreSystemRegistry
│   按依赖顺序管理所有 ICoreModuleSystem
│
├── Hook 触发器（每个状态转换时，按依赖顺序驱动系统）
│   OnBootStart / OnLoadStart / OnLoadScene / OnLoadComplete
│   OnMainMenuEnter / OnMainMenuExit
│   OnInLevelEnter / OnInLevelExit / OnGameQuit
│
└── 事件订阅（InLevel 状态下监听关键流程事件）
    LevelClearEvent → RequestLoadLevel(nextLevel)
    PlayerDeathEvent → [根据游戏设计决定]
```

---

## 二、旧系统问题归纳

### 问题 1：没有系统注册机制，GameCore 与系统完全割裂

各系统（`VgAudioManager`、`LevelManager` 等）都是孤立的持久单例。`GameCore` 无法枚举系统，无法按依赖初始化，也无法跨 Assembly 直接驱动（如 `LevelManager`）。最终导致用事件绕行，形成"假解耦"。

### 问题 2：`ILevelService` 这类"专用接口"是治标不治本

为 `LevelManager` 开 `ILevelService`，为 `PlayerManageSystem` 再开 `IPlayerService`……每加一个跨 Assembly 的系统就要加一个接口。根本原因是没有统一的系统注册与 Hook 机制。

### 问题 3：`CoreModuleManagerBase` 职责混乱

```
CoreModuleManagerBase<T, TLoadInfo, TLoader>
  ├── 职责 A：持久单例管理   ✓ 应保留
  ├── 职责 B：订阅 LoadRequestEvent  ✗ 应删除
  └── 职责 C：AddComponent<TLoader>  ✗ 应删除
```

所有系统被迫参与加载事件系统，即使与加载毫无关系。

### 问题 4：加载系统混合了两种范式

```
触发：Publish(LoadRequestEvent)       ← 事件驱动（无法直接 await）
执行：LoadAsync() BeforeLoad→Init     ← async 流水线（可 await）
```

调用方（`LoadRequestCommand.ExecuteAsync()`）实际上是在等 `MessageBroker` 的 Complete 信号，而不是等 `LoadAsync()` 本身，有竞态隐患且难以追踪。

### 问题 5：`LoadPipeline` + `ILoader` 的分层是伪抽象

`LoadPipeline` 把加载分为 6 个阶段，通过 `ILoader` 让系统参与。但这 6 个阶段本质上就是 Hook：OnLoadScene、OnLoadResource……与其多一个 `ILoader` 抽象层，不如直接用 `ICoreModuleSystem` 的 Hook，消除这一层。

### 问题 6：`LoaderType` 枚举跨 Assembly 污染

```csharp
// Core assembly 中定义了 GameMain 的东西
public enum LoaderType { SceneLoader, ProgressBar, Camera, LevelLoader, SaveLoader }
```

每次其他 Assembly 新增 Loader 都要修改 Core，违反开闭原则。

### 问题 7：Loader 是 `MonoBehaviour`，但不需要是

每次加载都 `AddComponent<TLoader>()` 再 `Destroy(loader)`，完全是 Unity 生命周期的误用。加载逻辑与 Unity 对象生命周期无关。

### 问题 8：命名不一致（Manager 与 System 混用）

| 当前名 | 新名 |
|---|---|
| `VgAudioManager` | `VgAudioSystem` |
| `VgInputManager` | `VgInputSystem` |
| `SaveManager` | `VgSaveSystem` |
| `VgLoadProgressManager` | `VgLoadingSplashManager` |

---

## 三、新设计

### 3.1 系统间通信模型详解

**以"玩家到达存档点"为例：**

```
[存档点脚本]
  触发: MessageBroker.Publish(new SavePointReachedEvent(savePointId))
                    │
         ┌──────────┴──────────┐
         ▼                     ▼
  [VgSaveSystem]          [LevelManager]
  订阅: SavePointReachedEvent    订阅: SavePointReachedEvent
  行为: 记录当前存档点           行为: 更新关卡进度
  再发: SaveDataUpdatedEvent
         │
         ▼
  [VgUiManager]
  订阅: SaveDataUpdatedEvent
  行为: 显示"已存档"提示

[GameCore] — 不订阅 SavePointReachedEvent
  （存档不是流程转换，GameCore 不关心）
```

关键点：
- GameCore **不参与**存档逻辑，它只关心游戏流程的状态转换
- 系统之间通过 MessageBroker 平等广播，不通过 GameCore 中转
- 但 GameCore 会订阅**流程相关**事件，如 `LevelClearEvent`（关卡通关）→ 驱动下一个状态

---

### 3.2 工程目录结构

```
Assets/
├── Core/
│   ├── GameCore/
│   │   ├── GameCore.cs
│   │   ├── GameCore.Flow.cs          FSM + Hook 触发逻辑
│   │   ├── GameCore.Systems.cs       系统注册与管理
│   │   ├── SceneEntryPoint.cs
│   │   └── GameFlow/
│   │       ├── GameFlowState.cs
│   │       ├── GameFlowTrigger.cs
│   │       ├── LoadContext.cs
│   │       ├── IBootStep.cs          启动序列步骤接口（仍保留）
│   │       ├── BootStepKey.cs        → 改为由系统注册，枚举废弃
│   │       └── Steps/
│   │           ├── ShowLogoStep.cs
│   │           └── LoadMainMenuSceneStep.cs
│   │
│   ├── GameCoreSystems/              新增：Core Assembly 的系统
│   │   ├── ICoreModuleSystem.cs      核心接口
│   │   ├── IGameCoreHookRegistry.cs  Hook 注册接口
│   │   ├── CoreSystemRegistry.cs    注册表 + 拓扑排序
│   │   ├── VgSceneManager.cs
│   │   ├── VgAudioSystem.cs          改名
│   │   ├── VgLoadingSplashManager.cs 改名
│   │   ├── VgUiManager.cs
│   │   ├── VgCameraManager.cs
│   │   ├── VgInputSystem.cs          改名
│   │   └── VgSaveSystem.cs           改名
│   │
│   ├── SceneLoading/                 新增：场景加载工具（非 Pipeline）
│   │   ├── SceneLoadInfo.cs          保留
│   │   └── SceneLoader.cs            改为纯 C# 工具类（internal）
│   │
│   └── [删除] CoreModule/CoreModuleLoader/
│
└── GameMain/
    ├── GameCoreSystems/              新增：GameMain Assembly 的系统
    │   ├── LevelManager.cs
    │   └── PlayerManageSystem.cs
    └── ...
```

---

### 3.3 `ICoreModuleSystem` 接口

```csharp
// Assets/Core/GameCoreSystems/ICoreModuleSystem.cs
namespace Core
{
    public interface ICoreModuleSystem
    {
        string SystemName { get; }

        // 声明此系统依赖的其他系统类型
        // CoreSystemRegistry 据此进行拓扑排序，决定初始化顺序和 Hook 执行顺序
        Type[] Dependencies { get; }

        // 向 GameCore 注册此系统对各生命周期 Hook 的响应
        void RegisterHooks(IGameCoreHookRegistry registry);
    }
}
```

---

### 3.4 `IGameCoreHookRegistry` 接口

GameCore 实现此接口，系统在 `RegisterHooks` 中订阅关心的 Hook：

```csharp
// Assets/Core/GameCoreSystems/IGameCoreHookRegistry.cs
namespace Core
{
    public interface IGameCoreHookRegistry
    {
        // ── 启动阶段 ──────────────────────────────────────────────
        // 各系统初始化自身内部状态（按依赖顺序执行）
        void OnSystemInit(Func<UniTask> handler);

        // 仅在生产启动路径触发（Booting 状态），独立运行路径不触发
        // 用于：显示 Logo、加载主菜单场景等一次性启动步骤
        void OnBootStart(Func<UniTask> handler, int order = 0);

        // ── 加载阶段（MainMenu↔InLevel 转换时触发）──────────────────
        // Loading 状态 Enter 时，依次触发以下 Hook
        void OnLoadStart(Func<LoadContext, UniTask> handler);   // 显示 Splash、暂停音频
        void OnLoadScene(Func<LoadContext, UniTask> handler);   // 加载场景、关卡资源
        void OnLoadComplete(Func<LoadContext, UniTask> handler); // 隐藏 Splash、启动关卡音乐

        // ── 主菜单阶段 ────────────────────────────────────────────
        void OnMainMenuEnter(Func<UniTask> handler);  // 显示主菜单 UI、播放菜单音乐
        void OnMainMenuExit(Func<UniTask> handler);   // 隐藏主菜单 UI、淡出音乐

        // ── 关卡阶段 ─────────────────────────────────────────────
        void OnInLevelEnter(Func<LoadContext, UniTask> handler); // 显示 HUD、生成玩家
        void OnInLevelExit(Func<UniTask> handler);              // 隐藏 HUD、销毁玩家

        // ── 退出 ─────────────────────────────────────────────────
        void OnGameQuit(Func<UniTask> handler); // 保存数据、清理资源
    }
}
```

**Hook 执行规则：**
- 同一 Hook 内，所有处理器按 `CoreSystemRegistry` 的拓扑排序顺序**顺序**执行（保证依赖先于被依赖方）
- 例外：`OnBootStart` 按 `order` 参数额外排序，用于控制 Logo 与场景加载的先后

---

### 3.5 `LoadContext`

游戏流程转换时携带的上下文，通过 Hook 参数传递给系统：

```csharp
// Assets/Core/GameCore/GameFlow/LoadContext.cs
namespace Core
{
    public readonly struct LoadContext
    {
        public readonly GameFlowState Destination;  // InLevel 或 MainMenu
        public readonly string ChapterId;
        public readonly string LevelId;
        public readonly int SpawnIndex;
        public readonly string SavePointName;       // 存档点加载时使用（可选）
        public readonly bool IsStandalone;          // 开发者独立运行模式

        public static LoadContext ForLevel(string chapterId, string levelId, int spawnIndex)
            => new(GameFlowState.InLevel, chapterId, levelId, spawnIndex, null, false);

        public static LoadContext ForSavePoint(string savePointName)
            => new(GameFlowState.InLevel, null, null, 0, savePointName, false);

        public static LoadContext ForMainMenu()
            => new(GameFlowState.MainMenu, null, null, 0, null, false);

        public static LoadContext ForStandalone(string chapterId, string levelId, int spawnIndex)
            => new(GameFlowState.InLevel, chapterId, levelId, spawnIndex, null, true);
    }
}
```

---

### 3.6 `CoreSystemRegistry`

```csharp
// Assets/Core/GameCoreSystems/CoreSystemRegistry.cs
public class CoreSystemRegistry : IGameCoreHookRegistry
{
    // 系统注册
    public void Register(ICoreModuleSystem system)
    {
        m_Systems[system.GetType()] = system;
        system.RegisterHooks(this);
    }

    // 拓扑排序后顺序执行 OnSystemInit
    public async UniTask InitializeAllAsync()
    {
        foreach (var system in GetSortedSystems())
        {
            CLogger.LogInfo($"[Registry] Init: {system.SystemName}", LogTag.GameCoreStart);
            foreach (var handler in m_InitHandlers[system.GetType()])
                await handler();
        }
    }

    // GameCore 在各状态转换时调用
    public async UniTask FireOnLoadStart(LoadContext ctx)   => await FireAll(m_LoadStartHandlers, ctx);
    public async UniTask FireOnLoadScene(LoadContext ctx)   => await FireAll(m_LoadSceneHandlers, ctx);
    public async UniTask FireOnLoadComplete(LoadContext ctx)=> await FireAll(m_LoadCompleteHandlers, ctx);
    public async UniTask FireOnMainMenuEnter()              => await FireAll(m_MainMenuEnterHandlers);
    public async UniTask FireOnMainMenuExit()               => await FireAll(m_MainMenuExitHandlers);
    public async UniTask FireOnInLevelEnter(LoadContext ctx)=> await FireAll(m_InLevelEnterHandlers, ctx);
    public async UniTask FireOnInLevelExit()               => await FireAll(m_InLevelExitHandlers);
    public async UniTask FireOnGameQuit()                  => await FireAll(m_GameQuitHandlers);
    public async UniTask FireOnBootStart()                 => await FireBootHandlers();

    // IGameCoreHookRegistry 实现（系统调用此接口注册 Handler）
    public void OnSystemInit(Func<UniTask> h)                    => Register(m_InitHandlers, h);
    public void OnBootStart(Func<UniTask> h, int order = 0)      => RegisterOrdered(m_BootHandlers, h, order);
    public void OnLoadStart(Func<LoadContext, UniTask> h)         => Register(m_LoadStartHandlers, h);
    public void OnLoadScene(Func<LoadContext, UniTask> h)         => Register(m_LoadSceneHandlers, h);
    public void OnLoadComplete(Func<LoadContext, UniTask> h)      => Register(m_LoadCompleteHandlers, h);
    public void OnMainMenuEnter(Func<UniTask> h)                 => Register(m_MainMenuEnterHandlers, h);
    public void OnMainMenuExit(Func<UniTask> h)                  => Register(m_MainMenuExitHandlers, h);
    public void OnInLevelEnter(Func<LoadContext, UniTask> h)      => Register(m_InLevelEnterHandlers, h);
    public void OnInLevelExit(Func<UniTask> h)                   => Register(m_InLevelExitHandlers, h);
    public void OnGameQuit(Func<UniTask> h)                      => Register(m_GameQuitHandlers, h);

    // 字段省略（各 Handler 列表按拓扑顺序插入）
}
```

---

### 3.7 具体系统的 Hook 注册示例

```csharp
// VgLoadingSplashManager.cs
public class VgLoadingSplashManager : MonoSingletonPersistent<VgLoadingSplashManager>,
    ICoreModuleSystem
{
    public string SystemName => "VgLoadingSplashManager";
    public Type[] Dependencies => Array.Empty<Type>();

    public void RegisterHooks(IGameCoreHookRegistry registry)
    {
        registry.OnLoadStart(async (_) => { Show(); });
        registry.OnLoadComplete(async (_) => { Hide(); });
        // Boot 时也需要显示 Splash，order=0 最先执行
        registry.OnBootStart(async () => { Show(); }, order: 0);
    }
    // ... Show/Hide 逻辑
}

// LevelManager.cs（GameMain.RunTime assembly）
public class LevelManager : MonoSingletonPersistent<LevelManager>, ICoreModuleSystem
{
    public string SystemName => "LevelManager";
    public Type[] Dependencies => new[] { typeof(VgSceneManager), typeof(VgSaveSystem) };

    public void RegisterHooks(IGameCoreHookRegistry registry)
    {
        registry.OnLoadScene(async (ctx) =>
        {
            if (ctx.Destination == GameFlowState.InLevel)
            {
                if (!string.IsNullOrEmpty(ctx.SavePointName))
                    await StartLevel(ctx.SavePointName);
                else if (ctx.IsStandalone)
                    await SetupExistingLevel(ctx); // 不加载场景，只初始化当前场景的关卡
                else
                    await StartLevel(ctx.ChapterId, ctx.LevelId, ctx.SpawnIndex);
            }
        });

        registry.OnInLevelExit(async () =>
        {
            await EndLevel();
        });
    }
}

// VgAudioSystem.cs
public class VgAudioSystem : MonoSingletonPersistent<VgAudioSystem>, ICoreModuleSystem
{
    public string SystemName => "VgAudioSystem";
    public Type[] Dependencies => Array.Empty<Type>();

    public void RegisterHooks(IGameCoreHookRegistry registry)
    {
        registry.OnMainMenuEnter(async () => await PlayMenuBgm());
        registry.OnMainMenuExit(async () => await FadeOutBgm());
        registry.OnInLevelEnter(async (ctx) => await PlayLevelBgm(ctx.ChapterId));
        registry.OnInLevelExit(async () => await FadeOutBgm());
        registry.OnLoadStart(async (_) => await FadeOutBgm());
    }
}

// PlayerManageSystem.cs（GameMain.RunTime assembly）
public class PlayerManageSystem : MonoSingletonPersistent<PlayerManageSystem>, ICoreModuleSystem
{
    public string SystemName => "PlayerManageSystem";
    public Type[] Dependencies => new[] { typeof(LevelManager) }; // 需要关卡先准备好

    public void RegisterHooks(IGameCoreHookRegistry registry)
    {
        registry.OnInLevelEnter(async (ctx) => await SpawnPlayer(ctx));
        registry.OnInLevelExit(async () => DespawnPlayer());
    }
}
```

---

### 3.8 `CoreModuleManagerBase` 精简

```csharp
// Before：三个泛型参数，内含加载逻辑
public abstract class CoreModuleManagerBase<T, TLoadInfo, TLoader>
    : MonoSingletonPersistent<T> { ... }

// After：一个泛型参数，只负责单例生命周期，注册自身
public abstract class CoreModuleManagerBase<T> : MonoSingletonPersistent<T>
    where T : MonoSingletonPersistent<T>
{
    protected override void Awake()
    {
        base.Awake();
        if (this is ICoreModuleSystem system)
            GameCore.Instance.RegisterSystem(system);
    }
}
```

---

### 3.9 GameCore FSM（UnityHFSM）

#### GameFlowTrigger

```csharp
public enum GameFlowTrigger
{
    BootComplete,   // Booting → MainMenu
    StartGame,      // MainMenu → Loading（目标 InLevel）
    ExitToMenu,     // InLevel → Loading（目标 MainMenu）
    SwitchLevel,    // InLevel → Loading（目标 InLevel，不同关卡）
    LoadComplete,   // Loading → InLevel 或 MainMenu（由 dest 决定）
}
```

#### 完整转换表

| From | Trigger | Guard | To |
|---|---|---|---|
| `None` | Ghost | — | `Booting` |
| `Booting` | `BootComplete` | — | `MainMenu` |
| `MainMenu` | `StartGame` | — | `Loading` |
| `InLevel` | `ExitToMenu` | — | `Loading` |
| `InLevel` | `SwitchLevel` | — | `Loading` |
| `Loading` | `LoadComplete` | `dest == InLevel` | `InLevel` |
| `Loading` | `LoadComplete` | `dest == MainMenu` | `MainMenu` |

#### `GameCore.Flow.cs` 关键逻辑

```csharp
private void InitFlow()
{
    m_Fsm = new StateMachine<string, GameFlowState, GameFlowTrigger>();

    m_Fsm.AddState(GameFlowState.None, isGhostState: true);

    m_Fsm.AddState(GameFlowState.Booting,
        onEnter: _ => RunBooting().Forget());

    m_Fsm.AddState(GameFlowState.MainMenu,
        onEnter: _ => m_Systems.FireOnMainMenuEnter().Forget(),
        onExit:  _ => m_Systems.FireOnMainMenuExit().Forget());

    m_Fsm.AddState(GameFlowState.Loading,
        onEnter: _ => RunLoading().Forget());

    m_Fsm.AddState(GameFlowState.InLevel,
        onEnter: _ => SubscribeInLevelEvents(),
        onExit:  _ => UnsubscribeInLevelEvents());

    // 转换定义
    m_Fsm.AddTriggerTransition(GameFlowTrigger.BootComplete,  GameFlowState.Booting,  GameFlowState.MainMenu);
    m_Fsm.AddTriggerTransition(GameFlowTrigger.StartGame,     GameFlowState.MainMenu, GameFlowState.Loading);
    m_Fsm.AddTriggerTransition(GameFlowTrigger.ExitToMenu,    GameFlowState.InLevel,  GameFlowState.Loading);
    m_Fsm.AddTriggerTransition(GameFlowTrigger.SwitchLevel,   GameFlowState.InLevel,  GameFlowState.Loading);
    m_Fsm.AddTriggerTransition(GameFlowTrigger.LoadComplete,  GameFlowState.Loading,  GameFlowState.InLevel,
        condition: _ => m_LoadContext.Destination == GameFlowState.InLevel);
    m_Fsm.AddTriggerTransition(GameFlowTrigger.LoadComplete,  GameFlowState.Loading,  GameFlowState.MainMenu,
        condition: _ => m_LoadContext.Destination == GameFlowState.MainMenu);

    m_Fsm.SetStartState(GameFlowState.None);
    m_Fsm.Init();
}

private async UniTask RunBooting()
{
    // 1. 初始化所有已注册系统
    await m_Systems.InitializeAllAsync();
    // 2. 执行启动序列（Logo、加载主菜单场景）
    await m_Systems.FireOnBootStart();
    m_Fsm.Trigger(GameFlowTrigger.BootComplete);
}

private async UniTask RunLoading()
{
    await m_Systems.FireOnLoadStart(m_LoadContext);
    await m_Systems.FireOnLoadScene(m_LoadContext);   // LevelManager 在此加载关卡
    await m_Systems.FireOnLoadComplete(m_LoadContext);
    m_Fsm.Trigger(GameFlowTrigger.LoadComplete);
}

// 关卡内订阅流程相关事件（GameCore 作为观察者）
private void SubscribeInLevelEvents()
{
    m_InLevelSubs = new CompositeDisposable(
        MessageBroker.Global.Subscribe<LevelClearEvent>(OnLevelClear)
        // 可扩展：PlayerDeathEvent 等
    );
    m_Systems.FireOnInLevelEnter(m_LoadContext).Forget();
}

private void UnsubscribeInLevelEvents()
{
    m_InLevelSubs?.Dispose();
    m_Systems.FireOnInLevelExit().Forget();
}

private void OnLevelClear(LevelClearEvent e)
{
    // GameCore 接收到关卡通关事件，决定下一步流程
    RequestLoadLevel(e.NextChapterId, e.NextLevelId, 0);
}

// ── 公开 API ──────────────────────────────────────────────────
public void Send(IGameFlowCommand command)
{
    CLogger.LogInfo($"[GameCore] >> {command.CommandName}", LogTag.GameCoreStart);
    command.Execute().Forget();
}

public void RequestLoadLevel(string chapterId, string levelId, int spawnIndex = 0)
{
    m_LoadContext = LoadContext.ForLevel(chapterId, levelId, spawnIndex);
    m_Fsm.Trigger(CurrentState == GameFlowState.InLevel
        ? GameFlowTrigger.SwitchLevel
        : GameFlowTrigger.StartGame);
}

public void RequestLoadLevelFromSavePoint(string savePointName)
{
    m_LoadContext = LoadContext.ForSavePoint(savePointName);
    m_Fsm.Trigger(CurrentState == GameFlowState.InLevel
        ? GameFlowTrigger.SwitchLevel
        : GameFlowTrigger.StartGame);
}

public void RequestExitToMenu()
{
    m_LoadContext = LoadContext.ForMainMenu();
    m_Fsm.Trigger(GameFlowTrigger.ExitToMenu);
}
```

---

### 3.10 `SceneEntryPoint` 更新

移除 `StandaloneSteps`（BootStepKey 列表），改为直接声明独立运行的 `LoadContext`：

```csharp
public class SceneEntryPoint : MonoBehaviour
{
    [SerializeField] private GameFlowState m_TargetState = GameFlowState.Booting;

    // 独立运行时的关卡上下文（TargetState = InLevel 时有效）
    [SerializeField] private string m_StandaloneChapterId;
    [SerializeField] private string m_StandaloneLevelId;
    [SerializeField] private int    m_StandaloneSpawnIndex;

    private void Start()
    {
        GameCore.Instance.OnSceneEntryPointReady(this).Forget();
    }

    public LoadContext GetStandaloneContext()
        => LoadContext.ForStandalone(m_StandaloneChapterId, m_StandaloneLevelId, m_StandaloneSpawnIndex);
}
```

---

## 四、流程示例

### 示例 1：玩家在主菜单点击"继续游戏"

```
UI Button.OnClick()
  → GameCore.Send(new ContinueGameCommand())
    → ContinueGameCommand.Execute():
        savePoint = VgSaveSystem.Instance.GetLastSavePoint()
        GameCore.RequestLoadLevelFromSavePoint(savePoint.Name)
    → GameCore: m_LoadContext = ForSavePoint("Chapter1_SavePoint3")
    → GameCore: Trigger(StartGame)

FSM: MainMenu → Loading

MainMenu_Exit:
  → FireOnMainMenuExit()
    → VgUiManager: 隐藏主菜单 UI
    → VgAudioSystem: 淡出菜单音乐

Loading_Enter → RunLoading():
  → FireOnLoadStart(ctx):
    → VgLoadingSplashManager: Show（显示加载动画）
    → VgAudioSystem: FadeOutBgm

  → FireOnLoadScene(ctx):       ← 按依赖顺序执行
    → VgSaveSystem: 载入存档点相关存档数据      （无依赖，先执行）
    → VgSceneManager: 加载关卡场景              （无依赖）
    → LevelManager: StartLevel("Chapter1_SavePoint3")   （依赖 VgSaveSystem）
    → PlayerManageSystem: 不在此 Hook 注册（在 OnInLevelEnter）

  → FireOnLoadComplete(ctx):
    → VgLoadingSplashManager: Hide（隐藏加载动画）

  → Trigger(LoadComplete)

FSM: Loading → InLevel

InLevel_Enter:
  → SubscribeInLevelEvents()（GameCore 开始观察 LevelClearEvent 等）
  → FireOnInLevelEnter(ctx):
    → VgAudioSystem: PlayLevelBgm("Chapter1")
    → VgUiManager: 显示 HUD
    → PlayerManageSystem: SpawnPlayer at SavePoint3
```

---

### 示例 2：开发者直接在关卡场景 Play Mode

```
Unity Editor: Play from GameLevel_Chapter1.unity

GameEntry Prefab（持久单例）: 自动创建（MonoSingletonPersistent 懒加载）

SceneEntryPoint.Start():
  TargetState = InLevel
  StandaloneChapterId = "Chapter1"
  StandaloneLevelId   = "Level1"
  → GameCore.OnSceneEntryPointReady(this)

GameCore 检测到非 Booting 入口:
  IsBootedFromEntry = false
  ctx = SceneEntryPoint.GetStandaloneContext()  // IsStandalone = true

GameCore 执行 Standalone 路径:
  → InitializeAllAsync()（系统自行初始化，跳过 OnBootStart）
  → m_LoadContext = ctx
  → 直接 Trigger(StartGame)  // 或手动 ChangeState(Loading)

FSM: None → Loading（跳过 Booting 和 MainMenu）

Loading_Enter → RunLoading():
  → FireOnLoadStart: VgLoadingSplashManager.Show（可选，看偏好）

  → FireOnLoadScene(ctx where IsStandalone=true):
    → LevelManager: SetupExistingLevel(ctx)
      ← 检测到 IsStandalone，不加载场景，只初始化已存在的关卡对象
    → PlayerManageSystem: 不在 OnLoadScene 注册

  → FireOnLoadComplete: VgLoadingSplashManager.Hide

  → Trigger(LoadComplete)

FSM: Loading → InLevel

InLevel_Enter:
  → FireOnInLevelEnter:
    → VgAudioSystem: PlayLevelBgm（或静音，取决于开发偏好）
    → PlayerManageSystem: SpawnPlayer

游戏直接开始，不经过 Logo、主菜单
```

---

### 示例 3：游戏中输入控制台命令 `start_level Chapter2StartPoint`

```
控制台解析: "start_level Chapter2StartPoint"
  → new SwitchLevelCommand(savePointName: "Chapter2StartPoint")
  → GameCore.Send(cmd)
  → cmd.Execute():
      GameCore.RequestLoadLevelFromSavePoint("Chapter2StartPoint")
  → GameCore: m_LoadContext = ForSavePoint("Chapter2StartPoint")
  → GameCore: Trigger(SwitchLevel)  ← 当前在 InLevel，使用 SwitchLevel Trigger

FSM: InLevel → Loading

InLevel_Exit:
  → UnsubscribeInLevelEvents()
  → FireOnInLevelExit():
    → VgAudioSystem: FadeOutBgm
    → VgUiManager: 隐藏 HUD
    → PlayerManageSystem: DespawnPlayer

Loading_Enter → RunLoading():
  → FireOnLoadStart: VgLoadingSplashManager.Show

  → FireOnLoadScene(ctx where SavePointName="Chapter2StartPoint"):
    → VgSaveSystem: 解析 "Chapter2StartPoint" → (Chapter2, Level1, spawnIdx=2)
      [或 LevelManager 自行解析，取决于 SavePoint 数据归属]
    → VgSceneManager: 卸载当前场景，加载 Chapter2 场景
    → LevelManager: StartLevel resolved from SavePoint

  → FireOnLoadComplete: VgLoadingSplashManager.Hide

  → Trigger(LoadComplete)

FSM: Loading → InLevel（因为 dest == InLevel）

InLevel_Enter:
  → SubscribeInLevelEvents()
  → FireOnInLevelEnter:
    → VgAudioSystem: PlayLevelBgm("Chapter2")
    → VgUiManager: 显示 HUD
    → PlayerManageSystem: SpawnPlayer at Chapter2StartPoint 位置
```

---

## 五、废弃清单

| 废弃目标 | 替代方案 |
|---|---|
| `CoreModuleManagerBase<T, TLoadInfo, TLoader>` | `CoreModuleManagerBase<T>` + `ICoreModuleSystem` |
| `LoadManager.cs` | `CoreSystemRegistry.FireOnLoadScene()` |
| `ILoader` (接口) | `ICoreModuleSystem.RegisterHooks` 中的 `OnLoadScene` |
| `LoaderBase<TLoadInfo>` (MonoBehaviour) | 系统内部的纯 C# 工具类 |
| `LoaderType.cs`（枚举） | 不再需要 |
| `CoreModuleLoaderEvents.cs` | 不再需要 |
| `LoadRequestCommand.cs` | 不再需要 |
| `SendLoaderCommand.cs` | 不再需要 |
| `ILevelService.cs` | `ICoreModuleSystem` + `OnLoadScene` Hook |
| `BootStepKey.cs`（枚举） | `OnBootStart` Hook + order 参数 |
| `BootStepRegistry.cs` | `CoreSystemRegistry` |
| `GameFlowPipeline.cs` | `CoreSystemRegistry.FireOn*` |
| `CameraLoader/CameraLoadInfo` | `VgCameraManager` 直接初始化 |
| `ProgressBarLoader/ProgressBarLoadInfo` | `VgLoadingSplashManager.OnLoadStart` Hook |
| `CoreModuleLoader/` 整个目录 | `SceneLoading/` 目录（仅保留 SceneLoadInfo + 内部工具） |

---

## 六、实施顺序

每个 Phase 完成后，系统保持可编译可运行。

### Phase 1：基础类型（无依赖，可立刻开始）
- 新建 `GameFlowTrigger.cs`、`LoadContext.cs`
- 新建 `ICoreModuleSystem.cs`、`IGameCoreHookRegistry.cs`
- 新建 `CoreSystemRegistry.cs`（空壳，逐步填充）

### Phase 2：删除加载事件系统
- 删除 `LoaderType.cs`、`CoreModuleLoaderEvents.cs`
- 删除 `LoadRequestCommand.cs`、`SendLoaderCommand.cs`
- 删除 `LoadManager.cs`
- 将 `SceneLoader` 改为纯 C# 工具类，迁移至 `SceneLoading/`
- 删除 `ILoader`、`LoaderBase`（MonoBehaviour 版）

### Phase 3：`CoreModuleManagerBase` 精简
- 去掉三个泛型参数，改为 `CoreModuleManagerBase<T>`
- 移除所有 LoadEvent 订阅
- `Awake` 中自动调用 `GameCore.Instance.RegisterSystem(this)`

### Phase 4：各系统迁移与 Hook 注册
- 将系统文件迁移至 `GameCoreSystems/` 目录
- 实现 `ICoreModuleSystem`，完成 `RegisterHooks`
- 应用重命名
- 删除旧的 `LoadEventSubscription` 相关字段

### Phase 5：`GameCore.Flow.cs` 重写
- 引入 UnityHFSM，建立 Trigger 转换表
- 实现 `RunBooting`、`RunLoading`
- 实现 `SubscribeInLevelEvents`（观察者）
- 接入 `CoreSystemRegistry.FireOn*`
- `Update()` 调用 `m_Fsm.OnLogic()`

### Phase 6：清理与验证
- 删除 `CoreModuleLoader/` 旧目录
- 更新 BootSteps（改为通过 `OnBootStart` Hook 实现）
- 更新 `SceneEntryPoint`（移除 `StandaloneSteps` 列表）
- 验证三个流程示例全部正常运行

---

## 七、关键设计决策记录

| 决策 | 选择 | 原因 |
|---|---|---|
| Assembly 边界解法 | `ICoreModuleSystem` + Hook | 不需要为每个跨 Assembly 系统开专用接口 |
| 加载机制 | Hook（OnLoadScene）替代 LoadPipeline | 消除 ILoader 抽象层，减少系统复杂度 |
| 系统初始化顺序 | 拓扑排序 Dependencies | 显式声明比运行时发现更可维护 |
| Boot 步骤扩展 | `OnBootStart(handler, order)` | 保留顺序可控性，同时允许系统自行注册 |
| `LoadContext.IsStandalone` | bool 标志 | 系统自行决定如何处理独立运行，不硬编码逻辑 |
| InLevel 事件观察 | 状态 Enter/Exit 时订阅/取消订阅 | GameCore 只在相关状态下观察，避免无效响应 |
| SwitchLevel 独立 Trigger | 与 StartGame 分开 | 来源状态不同（InLevel vs MainMenu），逻辑区分清晰 |
