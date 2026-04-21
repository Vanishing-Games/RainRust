using System.Collections.Generic;
using Core;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

namespace GameMain.RunTime
{
    public class LevelRoom : MonoBehaviour
    {
        public void DeActivate()
        {
            if (VirtualCamera != null)
                VirtualCamera.Priority.Enabled = false;
        }

        public void Activate()
        {
            if (VirtualCamera == null)
            {
                CLogger.LogError("LevelRoom don't have a ref of virtual cam", LogTag.LevelRoom);
                return;
            }

            if (CameraMode == CameraMode.Follow)
            {
                VirtualCamera.Follow = GameMain.GetPlayer().transform;
            }
            else
            {
                VirtualCamera.Follow = null;
                VirtualCamera.LookAt = null;
            }

            VirtualCamera.Priority.Enabled = true;
            VirtualCamera.Priority.Value = LevelManager.Instance.GetCurrentMaxPriority();
        }

        public CameraMode CameraMode
        {
            get => m_CameraMode;
            set => m_CameraMode = value;
        }

        public Bounds BorderBounds
        {
            get => m_BorderBounds;
            set => m_BorderBounds = value;
        }

        public CinemachineCamera VirtualCamera
        {
            get => m_VirtualCamera;
            set => m_VirtualCamera = value;
        }

        public List<LevelRoom> Neighbors => m_Neighbors;

        [SerializeField, ReadOnly]
        private CameraMode m_CameraMode;

        [SerializeField, ReadOnly]
        private Bounds m_BorderBounds;

        [SerializeField, ReadOnly]
        private CinemachineCamera m_VirtualCamera;

        [SerializeField, ReadOnly]
        private List<LevelRoom> m_Neighbors = new();
    }
}
