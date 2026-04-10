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
                GameCore.Instance.RegisterSystem(system);
            }
        }
    }
}
