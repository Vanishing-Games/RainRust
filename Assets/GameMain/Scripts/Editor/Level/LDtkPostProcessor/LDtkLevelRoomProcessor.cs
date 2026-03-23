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
                    CLogger.LogInfo(
                        $"Post process LDtk level: {r.name}",
                        LogTag.LdtkRoomProcessor
                    )
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

        private void ApplyCameraModeFields(RoomContext ctx) =>
            ctx.Room.CameraMode = ctx.Level.TryGetComponent<LDtkFields>(out var fields)
                ? fields.GetEnum<CameraMode>("CameraMode")
                : ctx.Room.CameraMode;

        private RoomContext CreateVirtualCamera(RoomContext ctx)
        {
            var vCam = new GameObject(
                $"{ctx.Level.name}_VirtualCamera"
            ).AddComponent<CinemachineCamera>();
            vCam.transform.SetParent(ctx.Level.transform);
            ctx.VCam = vCam;
            return ctx;
        }

        private void ApplyCameraFollow(RoomContext ctx) =>
            ctx.VCam.Follow =
                ctx.Room.CameraMode == CameraMode.Fixed
                    ? null
                    : RunTime.GameMain.GetPlayer()?.transform;

        private void ApplyFixedCameraSettings(RoomContext ctx)
        {
            if (ctx.Room.CameraMode != CameraMode.Fixed)
                return;

            var bounds = ctx.Room.BorderBounds;

            ctx.VCam.transform.position = bounds.center + Vector3.back * 10f;
            ctx.VCam.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            ctx.VCam.Lens.OrthographicSize = 11.25f;
            ctx.VCam.Lens.NearClipPlane = 0.1f;
            ctx.VCam.Lens.FarClipPlane = 5000f;
        }

        private void ApplyConfinerSettings(RoomContext ctx)
        {
            var confiner = ctx.Level.gameObject.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = ctx.Level.TryGetComponent<PolygonCollider2D>(out var poly)
                ? poly
                : ctx.Level.GetComponentInChildren<PolygonCollider2D>();
        }
    }
}
