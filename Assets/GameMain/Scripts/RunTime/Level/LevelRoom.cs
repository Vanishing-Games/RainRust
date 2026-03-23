using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

namespace GameMain.RunTime
{
    public class LevelRoom : MonoBehaviour
    {
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

        public void SetActive(bool active)
        {
            if (VirtualCamera != null)
            {
                VirtualCamera.Priority = active ? 10 : 0;
            }
        }
    }
}
