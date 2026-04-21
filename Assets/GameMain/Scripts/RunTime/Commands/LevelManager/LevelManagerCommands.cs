using Core;
using Cysharp.Threading.Tasks;

namespace GameMain.RunTime
{
    public static class LevelManagerCommands
    {
        public class LoadLevelCommand : IGameFlowCommand
        {
            public LoadLevelCommand(string chapterId, string levelId)
            {
                m_ChapterId = chapterId;
                m_LevelId = levelId;
            }

            public UniTask Execute()
            {
                GameCore.Instance.RequestLoadLevel(m_ChapterId, m_LevelId);
                return UniTask.CompletedTask;
            }

            public string CommandName => $"load_level {m_ChapterId}/{m_LevelId}";

            private readonly string m_ChapterId;
            private readonly string m_LevelId;
        }

        public class StartLevelCommand : ITriggerCommand
        {
            public StartLevelCommand(string savePointName)
            {
                m_SavePointName = savePointName;
            }

            public bool Execute()
            {
                LevelManager.Instance.StartLevel(m_SavePointName);
                return true;
            }

            private readonly string m_SavePointName;
        }

        public class EndLevelCommand : ITriggerCommand
        {
            public bool Execute()
            {
                LevelManager.Instance.EndLevel();
                return true;
            }
        }
    }
}
