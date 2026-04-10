using Core;
using Cysharp.Threading.Tasks;

namespace GameMain.RunTime
{
    public static class GameFlowCommands
    {
        public class BackToMenuCommand : IGameFlowCommand
        {
            public UniTask Execute()
            {
                GameCore.Instance.RequestExitToMenu();
                return UniTask.CompletedTask;
            }

            public string CommandName => "game:menu";
        }

        public class StartGameCommand : IGameFlowCommand
        {
            public StartGameCommand(string chapterId, string levelId, int spawnPointIndex = 0)
            {
                m_ChapterId = chapterId;
                m_LevelId = levelId;
                m_SpawnPointIndex = spawnPointIndex;
            }

            public UniTask Execute()
            {
                GameCore.Instance.RequestLoadLevel(m_ChapterId, m_LevelId, m_SpawnPointIndex);
                return UniTask.CompletedTask;
            }

            public string CommandName =>
                $"game:start {m_ChapterId}/{m_LevelId}:{m_SpawnPointIndex}";

            private readonly string m_ChapterId;
            private readonly string m_LevelId;
            private readonly int m_SpawnPointIndex;
        }
    }
}
