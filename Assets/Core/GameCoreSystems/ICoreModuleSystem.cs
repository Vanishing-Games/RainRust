using System;

namespace Core
{
    public interface ICoreModuleSystem
    {
        string SystemName { get; }

        Type[] Dependencies { get; }

        void RegisterHooks(IGameCoreHookRegistry registry);
    }
}
