using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

namespace GameMain.Editor
{
    public class LDtkRoomProcessor : LDtkPostprocessor
    {
        private class RoomContext
        {
            public LDtkComponentLevel Level;
            public LevelRoom Room;
            public CinemachineCamera VCam = null;

            public RoomContext(LDtkComponentLevel level, LevelRoom levelRoom)
            {
                Level = level;
                Room = levelRoom;
            }
        }

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson) =>
            Result
                .Success(root)
                .Tap(r =>
                    CLogger.LogInfo($"Post process LDtk level: {r.name}", LogTag.LdtkRoomProcessor)
                )
                .Map(r => r.GetComponent<LDtkComponentLevel>())
                .Tap(OnProcessRoomCamera);

        private void OnProcessRoomCamera(LDtkComponentLevel level) =>
            Result
                .Success(new RoomContext(level, level.gameObject.AddComponent<LevelRoom>()))
                .Tap(ctx =>
                {
                    ctx.Room.BorderBounds = ctx.Level.BorderBounds;
                    EditorUtility.SetDirty(ctx.Room);
                })
                .Tap(ApplyCameraModeFields)
                .Map(CreateVirtualCamera)
                .Tap(ApplyCameraFollow)
                .Tap(ApplyFixedCameraSettings)
                .Tap(ApplyConfinerSettings);

        private void ApplyCameraModeFields(RoomContext ctx)
        {
            var bounds = ctx.Room.BorderBounds;

            // Rooms larger than 40*23 should use follow mode.
            ctx.Room.CameraMode =
                bounds.size.x > 40f && bounds.size.y > 23f ? CameraMode.Follow : CameraMode.Fixed;
            EditorUtility.SetDirty(ctx.Room);
        }

        private RoomContext CreateVirtualCamera(RoomContext ctx)
        {
            var vCam = new GameObject(
                $"{ctx.Level.name}_VirtualCamera"
            ).AddComponent<CinemachineCamera>();
            vCam.transform.SetParent(ctx.Level.transform);
            ctx.VCam = vCam;
            ctx.Room.VirtualCamera = vCam;

            // Default settings
            vCam.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            vCam.Lens.OrthographicSize = 11.25f; // This matches half of 22.5 (close to 23)
            vCam.Lens.NearClipPlane = 0.1f;
            vCam.Lens.FarClipPlane = 5000f;

            EditorUtility.SetDirty(ctx.Room);
            return ctx;
        }

        private void ApplyCameraFollow(RoomContext ctx)
        {
            if (ctx.Room.CameraMode == CameraMode.Follow)
                ctx.VCam.gameObject.AddComponent<CinemachinePositionComposer>();
        }

        private void ApplyFixedCameraSettings(RoomContext ctx)
        {
            if (ctx.Room.CameraMode != CameraMode.Fixed)
                return;

            var bounds = ctx.Room.BorderBounds;
            ctx.VCam.transform.position = bounds.center + Vector3.back * 10f;
        }

        private void ApplyConfinerSettings(RoomContext ctx)
        {
            // Find the LogicMap layer
            LDtkComponentLayer logicMapLayer = null;
            foreach (var layer in ctx.Level.LayerInstances)
            {
                if (layer != null && layer.Identifier == LDtkIdentifiers.LogicMap)
                {
                    logicMapLayer = layer;
                    break;
                }
            }

            if (logicMapLayer == null)
            {
                CLogger.LogWarn(
                    $"Level {ctx.Level.name} does not have a LogicMap layer.",
                    LogTag.LdtkRoomProcessor
                );
                return;
            }

            // Generate PolygonCollider2D for room boundary
            var collider = logicMapLayer.gameObject.GetComponent<PolygonCollider2D>();
            if (collider == null)
            {
                collider = logicMapLayer.gameObject.AddComponent<PolygonCollider2D>();
            }

            collider.isTrigger = true;

            // Set points to match level bounds (in local space of logicMapLayer)
            Bounds bounds = ctx.Level.BorderBounds;
            Vector3 localMin = logicMapLayer.transform.InverseTransformPoint(bounds.min);
            Vector3 localMax = logicMapLayer.transform.InverseTransformPoint(bounds.max);

            collider.points = new Vector2[]
            {
                new(localMin.x, localMin.y),
                new(localMax.x, localMin.y),
                new(localMax.x, localMax.y),
                new(localMin.x, localMax.y),
            };

            var confiner = ctx.VCam.gameObject.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = collider;

            EditorUtility.SetDirty(logicMapLayer);
            EditorUtility.SetDirty(ctx.VCam);
        }
    }
}
