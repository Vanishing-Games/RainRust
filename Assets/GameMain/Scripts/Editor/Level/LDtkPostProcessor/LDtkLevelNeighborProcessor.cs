using Core;
using Core.Extensions;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

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
            CLogger.LogVerbose("Find rooms: " + allRooms, LogTag.LDtkLevelRoomProcessor);
            foreach (var room in allRooms)
            {
                CLogger.LogVerbose("Processing Room:" + room, LogTag.LDtkLevelRoomProcessor);
                room.Neighbors.Clear();
                LevelTransition[] transitions = room.GetComponentsInChildren<LevelTransition>();
                foreach (var transition in transitions)
                {
                    room.Transitions.Add(transition);

                    if (transition.Target != null)
                    {
                        LevelRoom neighborRoom =
                            transition.Target.GetComponentInParent<LevelRoom>();
                        if (neighborRoom != null && neighborRoom != room)
                        {
                            if (!room.Neighbors.Contains(neighborRoom))
                            {
                                room.Neighbors.Add(neighborRoom);
                            }
                        }
                    }
                }
                CLogger.LogVerbose(
                    "End Processing Room:" + room + "With neighbors found:\n" + room.Neighbors,
                    LogTag.LDtkLevelRoomProcessor
                );

                EditorUtility.SetDirty(room);
            }
        }
    }
}
