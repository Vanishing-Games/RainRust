<!--
 * --------------------------------------------------------------------------------
 * Copyright (c) 2025 Vanishing Games. All Rights Reserved.
 * @Author: VanishXiao
 * @Date: 2025-10-30 16:25:44
 * @LastEditTime: 2026-04-10 05:07:52
 * --------------------------------------------------------------------------------
-->
> 《不尬的诗》
> 
> 自信满满写首诗
> 
> 坐到电脑前
> 
> 不出一刻钟
> 
> 写不出来
> 
> 糊弄一下提交上去
> 
> 还挺爽

# 资产命名规范

## 命名格式: 

**type_category_?subcategory_?action_?subcategory_001**

## 示例

```
ui_button_select
ui_button_shop_select
gp_proj_fire_hit_small_001
gp_proj_fire_hit_small_002
gp_booster_bomb_activate
mus_core_jungle_001
```

## 格式

- 使用snake_case
- 使用关键词大写来突出信息,如: mus_factory_main_STOP
- 使用camelCase来表示一个物体,如: enemy_fireDemon_death

## 要求

- 使用英文
- 简明扼要
- 层层嵌套: 按照从概括到具体的原则逐层嵌套
- 合理排序: 方便按照字母顺序合理且高效地对名称进行排序
- 统一数位: xxx 如 001
- 使用动词形式: bomb_activation vs. bomb_activate
- 使用正常时态: chest_destroyed vs. chest_destroy
- 保持单复数一致
- 使用游戏主题来命名: 不要使用机制来命名

## Tips

- 名称不要过长
- 适当使用描述词如 `loop` 表明音乐为循环
- 缩写必须在表中有
- 同一物体,团队用词要统一

## 缩写表

| 缩写      | 全称                 |
| ------- | ----------------------- |
| gp | gameplay         |
| plr    | player                 |
| char    | character |
| amb    | ambience                |
| mus      | music  |

## 反面例子

```
clip_01 # 没有上下文，不明所以
awesome_sound1 # 数字前没加下划线
boss_enemy_eggman # enemy 比 boss 更宽泛；应改用 enemy_boss_eggman
GreatArt_1 GreatArt_2 GreatArt_10 # 数位不一致
sfx_env_forest_daytime_birds_chirping_loop_ambient_lowIntensity_01.wav # 太长
```

## 代码开发规范

## 结构与命名
- **布局顺序**: 所有的字段 (Fields) 与属性 (Properties) 必须放在类的最底部。
- **私有成员**: 私有或内部成员变量必须以 `m_` 开头 (例如 `m_IsTransitioning`)。

## 事件系统规范 (Events)

### 物理组织与目录结构
- **Assembly 绑定**: 每个含有 `.asmdef` 的目录下必须建立 `Events` 文件夹。
- **镜像映射**: `Events` 文件夹内的子目录结构必须与其对应的业务代码目录结构**完全镜像映射**。
  - *示例*: 若业务代码位于 `GameMain/RunTime/Level/LevelManager.cs`，其对应的事件定义应位于 `GameMain/RunTime/Events/Level/LevelEvents.cs`。

### 文件与类命名规范
- **文件命名**: 仅与物理路径相关。文件名为 `Events/` 目录后的相对路径连加，并以 `Events.cs` 结尾。
  - *示例*: `Events/Level/` -> `LevelEvents.cs`
  - *示例*: `Events/UI/Common/` -> `UICommonEvents.cs`
- **内部组织**: 文件内部必须使用与文件名同名的 `static class` 包裹所有具体的事件定义。
- **事件命名**: 
  - 遵循 `[主体][动作]Event` 格式。
  - **生命周期事件**: 推荐使用 `Pre` 或 `On` 或 `Post` 前缀。
    - *示例*: `GamePreInitEvent`, `GameOnInitEvent`, `GamePostInitEvent`
  - 事件推荐定义为 `struct` 以优化性能。
  - 事件命名必须具备上下文独立性：事件的名称（Struct/Class Name）必须自包含完整的业务语义（主体+动作），严禁依赖外层包裹的 static class 名称来暗示其含义
- **命名空间**: 命名空间必须与该 `.asmdef` 定义的 **Root Namespace** 保持一致，不额外增加 `.Events` 后缀。

### 代码组织示例
```csharp
namespace GameMain.RunTime // 保持与 asmdef Root Namespace 一致
{
    public static class LevelEvents // 与文件名 LevelEvents.cs 一致
    {
        public struct LoadedEvent : IEvent { public int LevelIndex; }
        public struct UnloadedEvent : IEvent { }
    }
}
```

## 命令系统规范 (Commands)

### 物理组织与目录结构
- **Assembly 绑定**: 每个含有 `.asmdef` 的目录下必须建立 `Commands` 文件夹。
- **镜像映射**: `Commands` 文件夹内的子目录结构必须与其对应的业务代码目录结构**完全镜像映射**。

### 文件与类命名规范
- **文件命名**: 仅与物理路径相关。文件名为 `Commands/` 目录后的相对路径连加，并以 `Commands.cs` 结尾。
  - *示例*: `Commands/Level/` -> `LevelCommands.cs`
  - *示例*: `Commands/GameFlow/` -> `GameFlowCommands.cs`
- **内部组织**: 文件内部必须使用与文件名同名的 `public static class` 包裹所有具体的命令定义。
- **命令命名**: 
  - 遵循 `[动作][主体]Command` 格式。
    - *示例*: `LoadLevelCommand`, `StartGameCommand`
  - 命令必须具备上下文独立性：命令的名称必须自包含完整的业务语义，严禁依赖外层包裹的 static class 名称来暗示其含义。
- **命名空间**: 命名空间必须与该 `.asmdef` 定义的 **Root Namespace** 保持一致，不额外增加 `.Commands` 后缀。

### 代码组织示例
```csharp
namespace GameMain.RunTime // 保持与 asmdef Root Namespace 一致
{
    public static class LevelCommands // 与文件名 LevelCommands.cs 一致
    {
        public class LoadLevelCommand : ICommand { /* ... */ }
    }
}
```

## 逻辑与库
- **事件系统**: 使用 **R3** 作为事件库。
- **函数式编程 (FP)**: 优先考虑函数式编程风格。基础库（如 `Result`）位于 `Assets/Core/FP`。
- **日志输出**: 统一使用 `CLogger`。
    - 每次调用日志必须至少提供一个 `LogTag` 进行分类（例如 `LogTag.Game`）。

## 代码质量
- **注释规范**: 代码中不允许出现注释（除非是补充代码本身无法表达的必要信息）。
- **优化建议**: 严格遵守项目中配置的 **Roslynator** 优化建议。
- **代码格式化**: 必须使用 **CSharpier** 作为代码格式化工具。