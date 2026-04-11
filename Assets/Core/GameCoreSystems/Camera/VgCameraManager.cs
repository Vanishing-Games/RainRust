using System;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Core
{
    public class VgCameraManager : CoreModuleManagerBase<VgCameraManager>, ICoreModuleSystem
    {
        public string SystemName => "VgCameraManager";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnSystemInit(async () =>
            {
                if (m_MainCamera == null)
                {
                    m_MainCamera = Camera.main;
                }

                if (m_MainCamera != null && m_CinemachineBrain == null)
                {
                    m_CinemachineBrain = m_MainCamera.GetComponent<CinemachineBrain>();
                    if (m_CinemachineBrain == null)
                    {
                        m_CinemachineBrain =
                            m_MainCamera.gameObject.AddComponent<CinemachineBrain>();
                    }
                }

                if (m_LoadingCamera != null)
                {
                    m_LoadingCamera.gameObject.SetActive(false);
                }

                await UniTask.CompletedTask;
            });
        }

        public void SetLoadingCameraActive(bool active)
        {
            if (m_LoadingCamera != null)
            {
                m_LoadingCamera.gameObject.SetActive(active);
            }
            else
            {
                CLogger.LogWarn("Loading Camera is not set in VgCameraManager", LogTag.Loading);
            }
        }

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
