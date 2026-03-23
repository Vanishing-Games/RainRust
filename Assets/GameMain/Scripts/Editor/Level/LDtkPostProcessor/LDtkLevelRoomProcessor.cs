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
                .Tap(ctx => ctx.Room.BorderBounds = ctx.Level.BorderBounds)
                .Tap(ApplyCameraModeFields)
                .Map(CreateVirtualCamera)
                .Tap(ApplyCameraFollow)
                .Tap(ApplyFixedCameraSettings)
                .Tap(ApplyConfinerSettings);

        private void ApplyCameraModeFields(RoomContext ctx)
        {
            var bounds = ctx.Room.BorderBounds;
            // Requirement: rooms larger than 40*23 should use follow mode.
            if (bounds.size.x > 40f || bounds.size.y > 23f)
            {
                ctx.Room.CameraMode = CameraMode.Follow;
            }
            else
            {
                ctx.Room.CameraMode = ctx.Level.TryGetComponent<LDtkFields>(out var fields)
                    ? fields.GetEnum<CameraMode>("CameraMode")
                    : CameraMode.Fixed;
            }
        }

        private RoomContext CreateVirtualCamera(RoomContext ctx)
        {
            var vCam = new GameObject($"{ctx.Level.name}_VirtualCamera").AddComponent<CinemachineCamera>();
            vCam.transform.SetParent(ctx.Level.transform);
            ctx.VCam = vCam;
            ctx.Room.VirtualCamera = vCam;
            
            // Default settings
            vCam.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            vCam.Lens.OrthographicSize = 11.25f; // This matches half of 22.5 (close to 23)
            vCam.Lens.NearClipPlane = 0.1f;
            vCam.Lens.FarClipPlane = 5000f;
            
            return ctx;
        }

        private void ApplyCameraFollow(RoomContext ctx)
        {
            // Note: At import time, GetPlayer() might return null. 
            // The actual follow target should ideally be set at runtime or use a tag-based follow if Cinemachine supports it.
            // But since the request says "在导入时, 你应该根据需求设置好VirtualCamera", 
            // we set up the component state.
            if (ctx.Room.CameraMode == CameraMode.Follow)
            {
                // We don't have the player instance here, so we might need a runtime script to assign it,
                // or use Cinemachine's Target Group / Find with tag at runtime.
                // For now, we ensure the camera is configured for following.
                var framingTransposer = ctx.VCam.gameObject.AddComponent<CinemachinePositionComposer>();
                // Adjust composer settings if needed
            }
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
            // Even for fixed cameras, a confiner doesn't hurt, but for follow cameras it's essential.
            var confiner = ctx.VCam.gameObject.AddComponent<CinemachineConfiner2D>();
            
            // Get or create a collider for bounding
            CompositeCollider2D composite = ctx.Level.GetComponentInChildren<CompositeCollider2D>();
            if (composite != null)
            {
                confiner.BoundingShape2D = composite;
            }
            else
            {
                // Fallback to PolygonCollider2D if composite is not found
                confiner.BoundingShape2D = ctx.Level.GetComponentInChildren<PolygonCollider2D>();
            }
        }
    }
}
