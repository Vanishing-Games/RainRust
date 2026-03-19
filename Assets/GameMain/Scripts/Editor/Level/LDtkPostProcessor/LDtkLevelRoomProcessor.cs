using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using Unity.Cinemachine;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

namespace GameMain.Editor
{
    public class LDtkRoomProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            Core.Logger.LogInfo($"Post process LDtk level: {root.name}", LogTag.LdtkProcessor);
            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            OnProcessRoomCamera(level);
        }

        private void OnProcessRoomCamera(LDtkComponentLevel level)
        {
            var levelGo = level.gameObject;
            var room = level.gameObject.AddComponent<LevelRoom>();

            if (levelGo.TryGetComponent<LDtkFields>(out var fields))
                room.CameraMode = fields.GetEnum<CameraMode>("CameraMode");

            room.BorderBounds = level.BorderBounds;

            var virtualCameraGo = new GameObject(level.name + "_VirtualCamera");
            virtualCameraGo.transform.SetParent(levelGo.transform);
            var virtualCamera = virtualCameraGo.AddComponent<CinemachineCamera>();

            if (room.CameraMode == CameraMode.Fixed)
            {
                virtualCamera.Follow = null;
                virtualCamera.transform.position = room.BorderBounds.center + Vector3.back * 10f;

                float roomAspect = room.BorderBounds.size.x / room.BorderBounds.size.y;
                float screenAspect = (float)Screen.width / Screen.height;
                float targetOrthoSize = room.BorderBounds.size.y / 2f / 100f;

                if (roomAspect > screenAspect)
                {
                    targetOrthoSize = room.BorderBounds.size.x / screenAspect / 2f / 100f;
                }

                virtualCamera.Lens.OrthographicSize = targetOrthoSize;
            }
            else
            {
                virtualCamera.Follow = RunTime.GameMain.GetPlayer()?.transform;
            }

            var confiner = levelGo.AddComponent<CinemachineConfiner2D>();
            if (confiner != null)
            {
                if (!levelGo.TryGetComponent<PolygonCollider2D>(out var poly))
                    poly = levelGo.GetComponentInChildren<PolygonCollider2D>();

                if (poly != null)
                {
                    confiner.BoundingShape2D = poly;
                }
            }
        }
    }
}
