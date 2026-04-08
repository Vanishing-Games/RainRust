# Unity-Template — Claude Code 项目指南

**Company:** Vanishing Games  
**Unity Version:** 6000.3.1f1 (2026 LTS)  
**Render Pipeline:** URP 17.3.0  
**Platform Targets:** PC Standalone, MacOs

---

## 项目概述

2D 横版平台/房间制冒险游戏模板。核心特性：
- LDtk 驱动的关卡设计，房间制加载与过渡
- R3 响应式事件驱动架构
- Zenject 依赖注入
- 自研 RainRust 混合 2D 光照渲染管线（基于 URP）
- UniTask 异步系统

---

## 代码规范（严格遵守）

### 命名
- 私有/内部成员变量：`m_` 前缀，如 `m_IsTransitioning`
- 公开属性：PascalCase
- 资产命名：`type_category_?subcategory_?action_001`（snake_case，英文）

### 代码结构
- **布局顺序**：Constructor → Methods → Properties → Fields（字段放最底部）
- **禁止注释**：除非代码本身无法表达必要信息，否则不允许写注释, 这要求代码命名必须清晰明确
- **格式化工具**：必须用 **CSharpier**（`printWidth: 128`, tabs, Allman 括号风格）
- **静态分析**：遵守 Roslynator 配置（`.editorconfig` 中部分规则已禁用）

### 强制使用的库
- 事件系统：**R3**（禁止用 C# 原生 event/delegate 做跨系统通信）
- 函数式编程：`Assets/Core/FP/Result.cs`（`Result<T,E>` 类型）
- 日志：**CLogger** + **LogTag**（每次调用必须提供至少一个 `LogTag`）
- 异步：**UniTask**（禁止裸 `Task`）

---

## 关键架构

### 核心框架（`Assets/Core/`）
| 系统 | 路径 | 说明 |
|------|------|------|
| 事件总线 | `Core/EventSystem/MessageBroker.cs` | 全局 pub/sub，R3 驱动 |
| 日志 | `Core/Logger/CLogger.cs` | 分级 LogTag，条件编译 |
| 命令模式 | `Core/Command/` | `ICommand<T>`, `IUniTaskCommand<T>`, `IUndoableCommand<T>` |
| 加载系统 | `Core/Load/` | `CoreModuleManagerBase<T,TLoadInfo,TLoader>` |
| 状态机 | `Core/Common/StateMachine/StateMachine.cs` | 泛型 HSM，Safe/Overwrite 转换模式 |
| FP 基础 | `Core/FP/Result.cs` | `Result<T,E>` Either 模式 |
| 单例 | `Core/Mono/` | `Singleton<T>`, `MonoSingletonPersistent<T>` |
| 资源加载 | `Core/AddressableResourceLoader/` | Resources/Addressables 统一抽象 |
| 输入 | `Core/Input/VgInputManager.cs` | 统一输入管理 |

### 游戏逻辑（`Assets/GameMain/`）
| 系统 | 路径 | 说明 |
|------|------|------|
| 关卡管理 | `GameMain/Scripts/RunTime/Level/LevelManager.cs` | 房间加载/过渡/相机优先级（360行） |
| 房间 | `GameMain/Scripts/RunTime/Level/LevelRoom.cs` | 房间生命周期，Cinemachine 集成 |
| 关卡过渡 | `GameMain/Scripts/RunTime/Level/LevelTransition.cs` | 碰撞触发关卡跳转 |
| 玩家管理 | `GameMain/Scripts/RunTime/PlayerManager/PlayerManager.cs` | 场景玩家引用 |
| 游戏入口 | `GameMain/Scripts/RunTime/GameCoreInvoker/GameEntryInvoker.cs` | 发布 `GameEntryInitEvent` |

### 渲染（`Assets/Rendering/RainRust/`）
自研 URP ScriptableRendererFeature，Pipeline：JFA Init → JFA → RayTracing → Final Composite

---

## 场景结构

```
GameEntry.unity      → 初始化入口
GameStartScene.unity → 开始菜单
GameLevel1.unity     → 关卡1
GameLevel2.unity     → 关卡2
GameEndScene.unity   → 结算
```

---

## 物理层级

| Layer | 用途 |
|-------|------|
| 6 Player | 玩家 |
| 7 Entity | 实体/敌人 |
| 10 Wall | 墙壁碰撞 |
| 28 Volume | 体积触发器 |
| 29 BackGround | 背景 |

---

## CI/CD

**入口文件：** `.github/workflows/ci-cd-main.yml`  
**配置文件：** `Pipeline Config/pipeline-settings.json`

| 触发 | 分支 | 步骤 |
|------|------|------|
| PR → main | main | Format → Test → Build |
| PR → develop | develop | Format → Test |
| release/** push | release | Format → Test → Build → Deploy |
| 每周日 19:00 (北京) | — | Build |

**跳过关键词（commit message 中）：**
- `[SKIP CICD]` `[SKIP FORMAT]` `[SKIP TEST]` `[SKIP BUILD]`

---

## 依赖与工具链

| 工具 | 版本 | 用途 |
|------|------|------|
| .NET SDK | 10.0 | 构建/工具链 |
| CSharpier | 1.1.2 | 代码格式化（已全局安装） |
| csharp-ls | 0.22.0 | C# 语言服务器（已全局安装） |
| Zenject | — | 依赖注入 |
| DOTween | — | 补间动画 |
| Odin Inspector | — | Inspector 扩展与序列化 |
| FMOD | — | 音频引擎 |
| Easy Save 3 | — | 存档系统 |
| LDtk Unity | — | 关卡编辑器集成 |

---

## 分支规范

```
main        — 稳定版本
develop     — 开发集成
release/**  — 发版分支
{name}/*    — 个人开发分支（如 vanish/dosth）
{issue-id}-{description} — 功能分支（如 237-hybird-lighting-solution）
```

---

## 测试

- `Assets/Tests/Editor/` — EditMode 测试
- `Assets/Tests/PlayMode/` — PlayMode 测试
- 框架：Unity Test Framework 1.6.0
- PR 合并前 CI 强制通过

---

## 已安装 Claude 插件

| 插件 | 命令 | 用途 |
|------|------|------|
| csharp-lsp | （自动） | C# 代码智能补全与诊断 |
| commit-commands | `/commit` | 自动生成 commit 并推送 |
| code-review | `/code-review` | 自动 PR 代码审查 |
| feature-dev | `/feature-dev` | 结构化功能开发工作流 |
| claude-md-management | `/revise-claude-md` | 维护本文件 |
| pr-review-toolkit | （多个 agent） | 专项 PR 审查（测试覆盖/错误处理/类型设计等） |
