# VgSaveSystem

## 快速使用

### 1. 访问系统
系统通过 `CoreModuleManager` 进行生命周期管理，可通过单例访问：
```csharp
var saveSystem = VgSaveSystem.Instance;
```

### 2. 存取数据
支持存取普通类型、类、结构体等。

```csharp
// 更新/写入数据 (当前槽位)
saveSystem.UpdateSaveValue("PlayerLevel", 10);

// 更新/写入全局数据
saveSystem.UpdateSaveValue("Volume", 0.8f, isGlobal: true);

// 读取数据
int level = saveSystem.GetSaveValue("PlayerLevel", defaultValue: 1);
float volume = saveSystem.GetSaveValue("Volume", 1.0f, isGlobal: true);

// 检查 Key 是否存在
bool hasKey = saveSystem.HasKey("FirstTimePlayer");
```

### 3. 执行存档与加载
存档默认在游戏退出时自动保存当前槽位和全局存档。你也可以手动触发：

```csharp
// 异步保存当前槽位
await saveSystem.WriteSlotSaveAsync();

// 异步保存全局数据
await saveSystem.WriteGlobalSaveAsync();

// 加载指定槽位
await saveSystem.LoadSlotAsync("SaveSlot_01");
```

### 4. 监听存档事件
通过 `MessageBroker` 监听系统状态：

```csharp
// 监听存档加载完成事件
MessageBroker.Global.Subscribe<SaveSystemEvents.SaveOnLoadEvent>(evt => {
    var container = evt.Container;
    Debug.Log($"Loaded slot: {container.Meta.SlotName}");
});
```

## 数据结构

- **SaveContainer**: 包含 `SaveMeta` (元数据) 和 `Data` (键值对字典)。
- **SaveMeta**: 包含槽位名称、显示名称、总游玩时间、最后存档时间、版本号等。

## 配置项

在 `VgSaveSystem` 的 Inspector 面板中可以配置：
- **Save Mode**: `Editor` (保存在项目目录下) 或 `Runtime` (保存在 `persistentDataPath`)。
- **Root Path Type**: 根目录类型。
- **Save Folder**: 存档文件夹名称。
- **Extension**: 存档文件后缀（默认 `.json`）。

## 注意事项
- 所有存储的对象必须是可序列化的。
- 存档系统会自动处理类型转换，但建议在读取时提供合理的 `defaultValue`。
- 修改数据后，`IsSlotDirty` 或 `IsGlobalDirty` 会标记为 `true`，系统在合适时机或退出时会写入磁盘。
