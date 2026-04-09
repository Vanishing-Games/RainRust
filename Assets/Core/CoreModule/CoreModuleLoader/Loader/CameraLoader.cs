using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class CameraLoader : LoaderBase<CameraLoadInfo>
    {
        public override LoaderType GetLoaderType()
        {
            return LoaderType.Camera;
        }

        public override void InitLoader(CameraLoadInfo loadInfo)
        {
            m_VgCameraManager = VgCameraManager.Instance;
        }

        public override UniTask InitLoadedThings()
        {
            return UniTask.CompletedTask;
        }

        private VgCameraManager m_VgCameraManager;
    }
}
