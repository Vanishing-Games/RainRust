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
            // Core.Logger.LogInfo(
            //     $"Post process LDtk level: {root.name}",
            //     LogTag.LdtkLogicMapProcessor
            // );

            // LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            // if (level == null)
            //     return;

            // foreach (LDtkComponentLayer layer in level.LayerInstances)
            // {
            //     if (layer == null || layer.Identifier != LDtkIdentifiers.LogicMap)
            //         continue;

            //     LDtkComponentLayerIntGridValues intGridValues = layer.IntGrid;
            //     if (intGridValues == null)
            //     {
            //         Core.Logger.LogWarn(
            //             $"Layer {layer.Identifier} has no IntGrid values.",
            //             LogTag.LdtkLogicMapProcessor
            //         );
            //         continue;
            //     }

            //     Tilemap tilemap = intGridValues.GetComponent<Tilemap>();
            //     if (tilemap == null)
            //     {
            //         Core.Logger.LogWarn(
            //             $"Layer {layer.Identifier} has no Tilemap component.",
            //             LogTag.LdtkLogicMapProcessor
            //         );
            //         continue;
            //     }

            //     // Create a temporary tile for collision
            //     Tile collisionTile = ScriptableObject.CreateInstance<Tile>();
            //     collisionTile.name = "LogicCollisionTile";
            //     collisionTile.sprite = null;
            //     collisionTile.color = new Color(0, 0, 0, 0);
            //     collisionTile.colliderType = Tile.ColliderType.Grid;

            //     // Important: Add the tile to the asset so it's serialized and persisted
            //     ImportContext.AddObjectToAsset(
            //         $"CollisionTile_{level.name}_{layer.name}",
            //         collisionTile
            //     );

            //     int width = layer.CSize.x;
            //     int height = layer.CSize.y;
            //     int count = 0;

            //     for (int x = 0; x < width; x++)
            //     {
            //         for (int y = 0; y < height; y++)
            //         {
            //             // Local implementation of LDtkCoordConverter.ConvertCell
            //             // Unity Y = -LDtk Y + Height - 1
            //             int unityY = -y + height - 1;
            //             Vector3Int pos = new Vector3Int(x, unityY, 0);

            //             if (intGridValues.GetValue(pos) != 0)
            //             {
            //                 tilemap.SetTile(pos, collisionTile);
            //                 count++;
            //             }
            //         }
            //     }

            //     Core.Logger.LogInfo(
            //         $"Set {count} collision tiles for layer {layer.Identifier} in {level.name}",
            //         LogTag.LdtkLogicMapProcessor
            //     );
            // }
        }
    }
}
