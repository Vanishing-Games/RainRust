using Core;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameMain.Editor
{
    public class LDtkLogicMapProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            Core.Logger.LogInfo(
                $"Post process LDtk level: {root.name}",
                LogTag.LdtkLogicMapProcessor
            );

            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            if (level == null)
                return;

            foreach (LDtkComponentLayer layer in level.LayerInstances)
            {
                if (layer == null || layer.Identifier != LDtkIdentifiers.LogicMap)
                    continue;

                var intGridValues = layer.IntGrid;
                if (intGridValues == null)
                    continue;

                Tilemap tilemap = intGridValues.GetComponent<Tilemap>();
                if (tilemap == null)
                    continue;

                BoundsInt bounds = tilemap.cellBounds;
                foreach (Vector3Int pos in bounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos))
                    {
                        tilemap.SetColliderType(pos, Tile.ColliderType.Grid);
                    }
                }
            }
        }
    }
}
