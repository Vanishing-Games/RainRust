using Cysharp.Threading.Tasks;

namespace Core
{
    public class ShowLogoStep
    {
        public async UniTask Execute()
        {
            CLogger.LogInfo("[ShowLogoStep] Showing logo...", LogTag.GameCoreStart);
            await UniTask.Delay(1000);
            CLogger.LogInfo("[ShowLogoStep] Logo done.", LogTag.GameCoreStart);
        }
    }
}
