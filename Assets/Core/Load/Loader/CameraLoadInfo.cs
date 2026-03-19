using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CameraLoadInfo : ILoadInfo
    {
        public LoaderType GetNeededLoaderType()
        {
            return LoaderType.Camera;
        }
    }
}
