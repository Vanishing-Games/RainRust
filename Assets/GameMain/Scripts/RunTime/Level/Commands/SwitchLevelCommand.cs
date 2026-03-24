using UnityEngine;

namespace GameMain.RunTime
{
    public class SwitchLevelCommand : MonoBehaviour
    {
        private readonly string m_ChapterId;
        private readonly string m_LevelId;
        private readonly int m_LevelSpawnPointIndex;

        public SwitchLevelCommand(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            m_ChapterId = chapterId;
            m_LevelId = levelId;
            m_LevelSpawnPointIndex = levelSpawnPointIndex;
        }

        public bool Execute()
        {
            LevelManager.Instance.StartLevel(m_ChapterId, m_LevelId, m_LevelSpawnPointIndex);
            return true;
        }
    }
}
