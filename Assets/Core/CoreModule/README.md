# CoreModule 架构设计与编码规范

本目录包含了项目的核心模块基类与管理逻辑，采用了基于**命令模式 (Command Pattern)** 和 **事件驱动 (Event-Driven)** 的模块化设计。

## 1. 核心设计逻辑：Manager-Loader 模型

核心模块遵循 `CoreModuleManagerBase<T, TLoadInfo, TLoader>` 的设计模式，旨在将“状态管理”与“加载逻辑”分离。

### 核心组件
- **Manager (模块管理器)**:
    - 继承自 `CoreModuleManagerBase`。
    - 职责：作为单例存在，维护模块的持久状态，监听全局加载事件，管理模块的生命周期。
    - 规范：仅处理顶层业务逻辑，不参与具体的 I/O 或耗时资源加载。
- **LoadInfo (加载契约)**:
    - 实现 `ILoadInfo` 接口。
    - 职责：定义该模块加载时所需的所有参数（如路径、ID、配置）。
    - 规范：必须指明对应的 `LoaderType`。
- **Loader (加载执行器)**:
    - 继承自 `LoaderBase<TLoadInfo>`。
    - 职责：实现具体的异步加载逻辑（如 `LoadResource`, `LoadScene`）。
    - 规范：由 Manager 动态挂载到同一 GameObject 上，任务完成后会被自动销毁（由加载系统管理）。

## 2. 交互逻辑：命令模式 (Command Pattern)

为了实现高内聚低耦合，外部调用模块功能时应优先使用 Command，而不是直接操作 Manager 单例。

- **ITriggerCommand**: 用于简单的触发式逻辑（如 `StartLevelCommand`）。
- **IUniTaskCommand**: 用于需要等待返回结果的异步逻辑。
- **Pipeline Execution**: 通过 `LoadRequestCommand` 发起全局事件，由 `LoadManager` 统一协调多个 Manager 的 Loader。

## 3. 控制台命令包装 (Console Command Wrapper)

为了方便调试，建议将常用的 Command 包装为控制台命令。

- **实现方式**: 创建静态类，并使用 `[ConsoleMethod]` 属性。
- **规范**: 
    - 存放路径: 对应模块下的 `ConsoleUtils` 文件夹。
    - 命名空间: 建议与模块保持一致。
    - 引用: 需确保所在的 `asmdef` 引用了 `IngameDebugConsole`。
- **示例**:
  ```csharp
  [ConsoleMethod("start_level", "Directly start a level")]
  public static void StartLevel(string chapterId, string levelId, int spawnIndex)
  {
      new StartLevelCommand(chapterId, levelId, spawnIndex).Execute();
  }
  ```

## 4. 编码规范

### 命名规范
- **私有变量**: 使用 `m_` 前缀（如 `m_LdtkProject`）。
- **公共属性**: 使用 PascalCase。
- **类名**: 核心模块建议以 `Vg` (Vanishing Games) 开头，通用功能直接命名。

### 日志规范
- **统一入口**: 必须使用 `CLogger`。
- **LogTag**: 每个模块必须在 `LogTag` 中定义自己的标识，调用时传入，以便在控制台过滤。
  ```csharp
  CLogger.Log("Message", LogTag.LevelManager);
  ```

### 异步规范
- **UniTask**: 整个架构深度集成 [UniTask](https://github.com/Cysharp/UniTask)，禁止使用传统的 `Coroutine` 或 `Thread`，优先使用 `async UniTask`。

### 依赖管理
- **单例模式**: 
    - `MonoSingletonPersistent<T>`: 场景切换不销毁（如 `LevelManager`）。
    - `MonoSingletonLasy<T>`: 懒加载单例。
- **解耦**: 模块间通信优先使用 `MessageBroker` (R3 EventBus)。

## 4. 扩展新模块步骤
1. 在 `LoaderType.cs` 注册新类型。
2. 定义 `LoadInfo` 类。
3. 定义 `Loader` 类，实现具体的加载阶段。
4. 创建 `Manager` 继承 `CoreModuleManagerBase`。
5. 创建对应的 `Command` 类供外部调用。
