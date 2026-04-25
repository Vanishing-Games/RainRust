using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public class LDtkLevelNeighborProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 11;

        protected override void OnPostprocessProject(GameObject root)
        {
            CLogger.LogInfo(
                $"Post process LDtk project for neighbor rooms: {root.name}",
                LogTag.LdtkRoomProcessor
            );

            LevelRoom[] allRooms = root.GetComponentsInChildren<LevelRoom>();

            foreach (var room in allRooms)
            {
                room.Neighbors.Clear();

                Bounds expanded = room.BorderBounds;
                expanded.Expand(new Vector3(1f, 1f, 1000f));

                foreach (var other in allRooms)
                {
                    if (other == room)
                        continue;
                    if (expanded.Intersects(other.BorderBounds))
                        room.Neighbors.Add(other);
                }

                EditorUtility.SetDirty(room);
            }
        }
    }
}
