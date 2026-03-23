using System;
using Cysharp.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// 游戏入口初始化命令
    /// 包含显示进度条、加载进度条资源、并跳转到 GameStartScene 的完整流程
    /// </summary>
    public class GameEntryInitCommand : IUniTaskCommand<bool>
    {
        public bool Execute()
        {
            ExecuteAsync().Forget();
            return true;
        }

        public async UniTask<bool> ExecuteAsync()
        {
            CLogger.LogInfo("[GameEntryInitCommand] Initiating Game...", LogTag.GameCoreStart);

            await InitProgressBar();

            using (var loadProgressManager = VgLoadProgressManager.Instance)
            {
                loadProgressManager.Show();

                LoadRequestEvent loadEvent = new("Loading Game Start Scene");
                loadEvent.AddLoadInfo(new SceneLoadInfo("GameStartScene"));
                var loadGameEntry = new LoadRequestCommand(loadEvent);

                bool loadCompleted = false;

                await UniTask.WhenAny(
                    loadGameEntry.ExecuteAsync(),
                    UniTask.Create(async () =>
                    {
                        while (!loadCompleted)
                        {
                            if (loadProgressManager.GetProgress() < 0.99f)
                                loadProgressManager.AddProgress(0.01f);

                            await UniTask.DelayFrame(1);
                        }
                    })
                );

                loadCompleted = true;
            }

            CLogger.LogInfo("[GameEntryInitCommand] Initiating Game Done", LogTag.GameCoreStart);
            return true;
        }

        private async UniTask InitProgressBar()
        {
            CLogger.LogInfo(
                "[GameEntryInitCommand] Initiating ProgressBar...",
                LogTag.GameCoreStart
            );

            var loadEvent = new LoadRequestEvent("Load Progress Bar");
            loadEvent.AddLoadInfo(new ProgressBarLoadInfo());
            var loadProgressBar = new LoadRequestCommand(loadEvent);

            await loadProgressBar.ExecuteAsync();

            CLogger.LogInfo(
                "[GameEntryInitCommand] Initiating ProgressBar Done",
                LogTag.GameCoreStart
            );
        }
    }
}
