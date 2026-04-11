using System;
using UnityEngine;

namespace Core
{
    public class VgUiManager : CoreModuleManagerBase<VgUiManager>, ICoreModuleSystem
    {
        public string SystemName => "VgUiManager";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry) { }
    }
}
