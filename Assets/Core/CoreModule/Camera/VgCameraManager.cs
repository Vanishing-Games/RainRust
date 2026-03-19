using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Core
{
    public class VgCameraManager
        : CoreModuleManagerBase<VgCameraManager, CameraLoadInfo, CameraLoader>
    {
        public override void RegisterLoadEvent()
        {
            base.RegisterLoadEvent();

            if (m_MainCamera == null)
                m_MainCamera = Camera.main;

            if (m_MainCamera != null && m_CinemachineBrain == null)
            {
                m_CinemachineBrain = m_MainCamera.GetComponent<CinemachineBrain>();
                if (m_CinemachineBrain == null)
                    m_CinemachineBrain = m_MainCamera.gameObject.AddComponent<CinemachineBrain>();
            }

            if (m_LoadingCamera != null)
                m_LoadingCamera.gameObject.SetActive(false);
        }

        public void SetLoadingCameraActive(bool active)
        {
            if (m_LoadingCamera != null)
            {
                m_LoadingCamera.gameObject.SetActive(active);
            }
            else
            {
                Logger.LogWarn(
                    "Loading Camera is not set in VgCameraManager",
                    LogTag.VgCameraManager
                );
            }
        }

        protected override LoaderType GetLoaderType() => LoaderType.Camera;

        protected override void OnLoadingError(Exception exception) { }

        [SerializeField]
        private Camera m_MainCamera;

        [SerializeField]
        private CinemachineBrain m_CinemachineBrain;

        [SerializeField]
        private Camera m_LoadingCamera;

        public Camera MainCamera => m_MainCamera;
        public CinemachineBrain CinemachineBrain => m_CinemachineBrain;
        public Camera LoadingCamera => m_LoadingCamera;
    }
}
