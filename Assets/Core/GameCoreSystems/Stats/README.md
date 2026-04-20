<!--
 * --------------------------------------------------------------------------------
 * Copyright (c) 2025 Vanishing Games. All Rights Reserved.
 * @Author: VanishXiao
 * @Date: 2026-03-24 21:56:04
 * @LastEditTime: 2026-04-21 00:30:47
 * --------------------------------------------------------------------------------
-->
调用示例

```c#
// 计数
StatsManager.Increment(StatKeys.PlayerJump);

// 设置最大值, 会自动处理, 选择现有的最大值和传入最大中的最大值
StatsManager.SetMax("HighScore", 100);

// 获取数值
float jumps = StatsManager.GetValue(StatKeys.PlayerJump);

// 监听变化
MessageBroker.Global.Receive<StatChangedEvent>()
    .Where(e => e.Key == StatKeys.PlayerJump)
    .Subscribe(e => CLogger.LogInfo($"Jumps: {e.NewValue}", LogTag.Game));
```