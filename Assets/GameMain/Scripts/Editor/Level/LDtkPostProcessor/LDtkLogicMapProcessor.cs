using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameMain.Editor
{
    /// <summary>
    /// 处理 LogicMap 层的碰撞生成。
    /// 在 LDtk 中，LogicMap 层通常用 0 表示墙体，但 LDtk 不会为 0 生成 Tile。
    /// 本处理器会将 LogicMap 层中的空位（值为 0 的格子）填充上默认的碰撞 Tile。
    /// </summary>
    public class LDtkLogicMapProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            if (level == null)
                return;

            // 查找 LogicMap 物体
            Transform logicMapTransform = root.transform.Find(LDtkIdentifiers.LogicMap);
            if (logicMapTransform == null)
            {
                // 如果直接找不到，尝试在子物体中递归查找（LDtkUnity 的层级结构有时会多一层 IntGrid）
                logicMapTransform = FindRecursive(root.transform, LDtkIdentifiers.LogicMap);
            }

            if (logicMapTransform == null)
                return;

            Tilemap tilemap = logicMapTransform.GetComponentInChildren<Tilemap>();
            if (tilemap == null)
                return;

            // 加载默认的 LDtk IntGrid Tile，确保它在 Unity 中配置了 Collision Type 为 Grid 或 Sprite
            TileBase wallTile = Resources.Load<TileBase>("LDtkDefaultTile");
            if (wallTile == null)
            {
                Debug.LogWarning(
                    "LDtkLogicMapProcessor: Could not load LDtkDefaultTile from Resources."
                );
                return;
            }

            // 获取关卡的格子范围
            // LDtkUnity 会根据关卡大小和格子大小设置 Tilemap 的坐标
            BoundsInt bounds = tilemap.cellBounds;

            // 遍历整个范围，如果某个格子没有 Tile，则认为是 0 (Wall)，填充它
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (!tilemap.HasTile(pos))
                    {
                        tilemap.SetTile(pos, wallTile);
                    }
                    else
                    {
                        // 如果原本有 Tile (LDtk 中的非 0 值)，则清除它，因为我们只想要 0 作为碰撞
                        // 注意：如果用户希望非 0 值也有碰撞，则不应清除。
                        // 根据用户需求：“地图为 0 的格子作为有碰撞的格子”，这里我们保留非 0 格子为空（或不作为碰撞）。
                        // 修正：如果 LogicMap 是 IntGrid，原本有 Tile 说明 LDtk 里涂了颜色。
                        // 如果用户希望 0 是墙，非 0 是路，那么我们应该清除原本的 Tile。
                        tilemap.SetTile(pos, null);
                    }
                }
            }

            // 刷新 Tilemap 物理
            TilemapCollider2D collider =
                logicMapTransform.GetComponentInChildren<TilemapCollider2D>();
            if (collider != null)
            {
                collider.ProcessTilemapChanges();
            }
        }

        private Transform FindRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                Transform found = FindRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
