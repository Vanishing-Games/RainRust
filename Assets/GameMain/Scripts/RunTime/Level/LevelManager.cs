using Core;
using LDtkUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelManager : MonoBehaviour
    {
        public void EnterLevel(string chapter, string level, uint levelSpawnPointIndex) { }

        public void ExitLevel() { }

        private void InitLevelManager()
        {
            if (m_LdtkProject != null)
                return;

            var ldtkProjects = FindObjectsByType<LDtkComponentProject>(
                FindObjectsSortMode.InstanceID
            );
            if (ldtkProjects.Length > 1)
                CLogger.LogError("Multiple LDtk projects find in scene", LogTag.LevelManager);

            if (ldtkProjects.Length == 0)
                CLogger.LogError("No LDtk Project was found", LogTag.LevelManager);
        }

        private LDtkComponentProject m_LdtkProject;
    }
}
