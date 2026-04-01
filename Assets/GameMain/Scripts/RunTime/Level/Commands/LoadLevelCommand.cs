using System.Collections.Generic;
using Core;

namespace GameMain.RunTime
{
    public class LoadLevelCommand : ITriggerCommand
    {
        private readonly string m_ChapterId;
        private readonly string m_LevelId;
        private readonly int m_LevelSpawnPointIndex;

        public LoadLevelCommand(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            m_ChapterId = chapterId;
            m_LevelId = levelId;
            m_LevelSpawnPointIndex = levelSpawnPointIndex;
        }

        public bool Execute()
        {
            var levelLoadInfo = new LevelLoadInfo(m_ChapterId, m_LevelId, m_LevelSpawnPointIndex);
            var loadRequestEvent = new LoadRequestEvent(
                $"LoadLevel: {m_LevelId}",
                new List<ILoadInfo> { levelLoadInfo },
                LoadRequestEvent.LoadSettings.Default
            );

            return new LoadRequestCommand(loadRequestEvent).Execute();
        }
    }
}
