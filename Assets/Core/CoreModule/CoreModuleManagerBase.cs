using UnityEngine;

namespace Core
{
    public abstract class CoreModuleManagerBase<T> : MonoSingletonPersistent<T>
        where T : MonoSingletonPersistent<T>
    {
        protected override void Awake()
        {
            base.Awake();
            if (this is ICoreModuleSystem system)
            {
                CLogger.LogInfo(
                    $"[CoreModuleManagerBase] Awake registering: {system.SystemName}",
                    LogTag.GameCoreStart
                );
                GameCore.Instance.RegisterSystem(system);
            }
        }
    }
}
