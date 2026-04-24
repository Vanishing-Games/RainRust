# LDtk Entity 导入管线说明 (LDtk Entity Pipeline)

此文件夹包含关卡导入时的自动化处理逻辑。该管线旨在实现 **数据驱动 (Data-Driven)**、**零运行时开销 (Zero Runtime Overhead)** 和 **强鲁棒性 (Robustness)**。

## 核心组件 (Core Components)

1. **LDtkFieldAttribute**: 标记在 C# 字段上，用于自动同步 LDtk 字段数据。
2. **LDtkEntity (Base)**: 所有实体的基类，处理 IID、Level/World 引用、缩放与锚点修正。
3. **LDtkTriggerEntity / LDtkSolidEntity**: 针对交互和物理实体的特化基类。
4. **LDtkAutoEntityProcessor**: 后处理核心，负责反射注入、Prefab 自动匹配和烘焙。

## 工作流 (Workflow)

### 1. 创建新实体
- 继承 `LDtkEntity` (或其子类)。
- 使用 `[LDtkField]` 标记需要同步的变量名。
- 如果变量名与 LDtk 不一致，使用 `[LDtkField("LDtk_Name")]`。

### 2. 配置 Prefab
- 将 Prefab 命名为与 LDtk Identifier 一致。
- 放入 `Assets/Prefabs/RunTime` 目录下的任何位置。
- 设置 `BasePixelSize` (如 16x16)，管线将根据此值计算 `localScale`。

### 3. 生命周期钩子
- **OnSyncFromLdtk**: 处理空间变换。
- **OnPostImport**: 在数据注入完成后调用，用于生成 ID、初始化视觉等。

## 空间计算规范 (Spatial Standard)

- **锚点**: LDtk 默认为左上角，管线会自动将其修正为 Unity 中心对齐。
- **缩放**: 统一采用 `localScale` 方案。`Scale = LDtk_Size / BasePixelSize`。
- **层级**: 所有生成的实体都会归位到关卡根节点的 `RuntimeEntities` 容器下。

## 注意事项 (Notes)

- **类型匹配**: 如果 LDtk 字段与 C# 类型不匹配（如 Float 注入 Int），处理器会报错并输出实体 IID。
- **引用解析**: 实体间的引用 (`EntityRef`) 建议在运行时通过 `RuntimeEntityRegistry` 进行最终解析。
