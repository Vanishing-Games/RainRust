using Core;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

namespace GameMain.RunTime
{
    public class LevelRoom : MonoBehaviour
    {
        public void DeActivate() { }

        public void Activate()
        {
            if (VirtualCamera == null)
                CLogger.LogError("LevelRoom don't have a ref of virtual cam", LogTag.LevelRoom);

            if (CameraMode == CameraMode.Follow)
                VirtualCamera.Follow = GameMain.GetPlayer().transform;

            VirtualCamera.Priority.Enabled = true;
            VirtualCamera.Priority.Value = LevelManager.Instance.GetCurrentMaxPriority();
        }

        [SerializeField, ReadOnly]
        private CameraMode _cameraMode;

        [SerializeField, ReadOnly]
        private Bounds _borderBounds;

        [SerializeField, ReadOnly]
        private CinemachineCamera _virtualCamera;

        public CameraMode CameraMode
        {
            get => _cameraMode;
            set => _cameraMode = value;
        }

        public Bounds BorderBounds
        {
            get => _borderBounds;
            set => _borderBounds = value;
        }

        public CinemachineCamera VirtualCamera
        {
            get => _virtualCamera;
            set => _virtualCamera = value;
        }
    }
}
