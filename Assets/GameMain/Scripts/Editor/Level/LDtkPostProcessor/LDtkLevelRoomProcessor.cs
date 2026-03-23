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
            var confiner = ctx.VCam.gameObject.AddComponent<CinemachineConfiner2D>();

            // Get or create a collider for bounding
            CompositeCollider2D composite = ctx.Level.GetComponentInChildren<CompositeCollider2D>();
            if (composite != null)
                confiner.BoundingShape2D = composite;
            else
                confiner.BoundingShape2D = ctx.Level.GetComponentInChildren<PolygonCollider2D>();
        }
    }
}
