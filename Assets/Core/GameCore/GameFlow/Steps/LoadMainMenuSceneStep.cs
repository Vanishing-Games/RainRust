using Cysharp.Threading.Tasks;

namespace Core
{
    public class LoadMainMenuSceneStep
    {
        public async UniTask Execute()
        {
            CLogger.LogInfo(
                "[LoadMainMenuSceneStep] Loading main menu scene...",
                LogTag.GameCoreStart
            );

            var sceneLoader = new SceneLoader();
            await sceneLoader.LoadScene(new SceneLoadInfo("GameStartScene"));

            CLogger.LogInfo(
                "[LoadMainMenuSceneStep] Main menu scene loaded.",
                LogTag.GameCoreStart
            );
        }
    }
}
