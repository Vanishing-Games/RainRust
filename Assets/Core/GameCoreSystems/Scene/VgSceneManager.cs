using System;
using Cysharp.Threading.Tasks;

namespace Core
{
    public class VgSceneManager : CoreModuleManagerBase<VgSceneManager>, ICoreModuleSystem
    {
        public string SystemName => "VgSceneManager";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnLoadScene(async ctx =>
            {
                if (ctx.Destination == GameFlowState.MainMenu)
                {
                    await m_Loader.LoadScene(new SceneLoadInfo("MainMenu"));
                }
                else if (ctx.Destination == GameFlowState.InLevel && !ctx.IsStandalone)
                {
                    if (!string.IsNullOrEmpty(ctx.ChapterId))
                    {
                        await m_Loader.LoadScene(new SceneLoadInfo("GameLevel"));
                    }
                }
            });
        }

        private readonly SceneLoader m_Loader = new();
    }
}
