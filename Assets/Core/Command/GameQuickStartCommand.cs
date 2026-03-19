using System;
using Cysharp.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// 游戏快速启动命令
    /// 用于非 GameEntry 场景的底层系统静默初始化
    /// </summary>
    public class GameQuickStartCommand : IUniTaskCommand<bool>
    {
        public bool Execute()
        {
            ExecuteAsync().Forget();
            return true;
        }

        public async UniTask<bool> ExecuteAsync()
        {
            Logger.LogInfo(
                "[GameQuickStartCommand] Quick Starting Core Systems...",
                LogTag.GameCoreStart
            );
            Logger.LogInfo("[GameQuickStartCommand] Quick Start Done", LogTag.GameCoreStart);
            return await UniTask.FromResult(true);
        }
    }
}
