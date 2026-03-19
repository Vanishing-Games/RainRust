using UnityEngine;

namespace GameMain.Editor
{
    public static class LDtkIdentifiers
    {
        // Layer Identifiers
        public const string LogicMap = "LogicMap"; // 逻辑地图层, 0: Wall,
        public const string Entities = "Entities"; // 实体层, 包含玩家、敌人、道具等
        public const string ManualTiles = "ManualTiles"; // 手动绘制的瓦片层, 包含装饰性元素
        public const string AutoTiles = "AutoTiles"; // 自动绘制的瓦片层, 包含地面、平台等

        // Entity Identifiers
        public const string LevelTransition = "LevelTransition"; // 关卡切换实体
    }
}
