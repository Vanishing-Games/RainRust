using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Scene
{
    /// <summary>
    /// AB Tile which alternates between two sprites to avoid visual seams.
    /// </summary>
    public class AbTile : Tile
    {
        public enum AbOrientation
        {
            Horizontal,
            Vertical,
        }

        public enum ATileDirection
        {
            Left,
            Right,
            Up,
            Down,
        }

        [Header("Sprites")]
        public Sprite spriteA;
        public Sprite spriteB;

        [Header("Configuration")]
        [Tooltip("Direction in which the AB pattern alternates.")]
        public AbOrientation abOrientation = AbOrientation.Horizontal;

        [Tooltip("The direction where the 'A' tile sequence starts from.")]
        public ATileDirection aTileDirection = ATileDirection.Left;

        public override void GetTileData(
            Vector3Int position,
            ITilemap tilemap,
            ref TileData tileData
        )
        {
            base.GetTileData(position, tilemap, ref tileData);

            int distance = GetDistanceFromStart(position, tilemap);
            tileData.sprite = (distance % 2 == 0) ? spriteA : spriteB;
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);

            // When a tile is placed or updated, we need to refresh the next tile in the sequence
            // to propagate the A/B parity change if necessary.
            Vector3Int nextDir = GetNextDirection();
            Vector3Int nextPos = position + nextDir;

            // Only refresh if the neighbor is the same tile asset
            if (tilemap.GetTile(nextPos) == this)
            {
                tilemap.RefreshTile(nextPos);
            }
        }

        private int GetDistanceFromStart(Vector3Int position, ITilemap tilemap)
        {
            Vector3Int searchDir = GetSearchDirection();
            int distance = 0;
            Vector3Int currentPos = position + searchDir;

            // Iteratively find the edge of the contiguous block of this type of tile
            while (tilemap.GetTile(currentPos) == this)
            {
                distance++;
                currentPos += searchDir;
            }

            return distance;
        }

        private Vector3Int GetSearchDirection()
        {
            // The search direction is towards the "Start" of the sequence.
            // It must align with the chosen orientation.
            if (abOrientation == AbOrientation.Horizontal)
            {
                return (aTileDirection == ATileDirection.Right)
                    ? Vector3Int.right
                    : Vector3Int.left;
            }
            else // Vertical
            {
                return (aTileDirection == ATileDirection.Up) ? Vector3Int.up : Vector3Int.down;
            }
        }

        private Vector3Int GetNextDirection()
        {
            // The next direction is opposite to the search direction.
            // Refreshing in this direction propagates changes away from the "Start".
            if (abOrientation == AbOrientation.Horizontal)
            {
                return (aTileDirection == ATileDirection.Right)
                    ? Vector3Int.left
                    : Vector3Int.right;
            }
            else // Vertical
            {
                return (aTileDirection == ATileDirection.Up) ? Vector3Int.down : Vector3Int.up;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/2D/Custom Tiles/AbTile")]
        public static void CreateAbTile()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save AbTile",
                "New AbTile",
                "Asset",
                "Save AbTile",
                "Assets"
            );
            if (path == "")
                return;

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AbTile>(), path);
        }
#endif
    }
}
