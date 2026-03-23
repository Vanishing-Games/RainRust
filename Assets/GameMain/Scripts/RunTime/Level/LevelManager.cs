using System;
using System.Collections.Generic;
using Core;
using LDtkUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelManager : CoreModuleManagerBase<LevelManager, LevelLoadInfo, LevelLoader>
    {
        protected override LoaderType GetLoaderType() => LoaderType.LevelLoader;

        protected override void OnLoadingError(Exception exception)
        {
            CLogger.LogError($"Level Load Error: {exception.Message}", LogTag.LevelManager);
        }

        /// <summary>
        /// 进入指定关卡
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="levelId"></param>
        /// <param name="levelSpawnPointIndex"></param>
        public void StartLevel(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            InitLevelManager();
            SetUp(chapterId, levelId, levelSpawnPointIndex);
            SetUpPlayer();
        }

        /// <summary>
        /// 退出关卡
        /// </summary>
        public void EndLevel() { }

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

            if (ldtkProjects.Length > 0)
                m_LdtkProject = ldtkProjects[0];
        }

        private void SetUp(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            m_CurrentWorld = GetChapeter(chapterId);
            if (m_CurrentWorld == null)
            {
                CLogger.LogError("Can't Find Chapter: " + chapterId, LogTag.LevelManager);
                return;
            }

            m_CurrentLevel = GetLevel(levelId);
            if (m_CurrentLevel == null)
            {
                CLogger.LogError("Can't Find Level: " + levelId, LogTag.LevelManager);
                return;
            }

            m_CurrentLevelTransition = GetLevelTransition(levelSpawnPointIndex);
            if (m_CurrentLevelTransition == null)
            {
                CLogger.LogError(
                    "Can't Find LevelTransition: " + levelSpawnPointIndex,
                    LogTag.LevelManager
                );
                return;
            }
        }

        private void SetUpPlayer()
        {
            var player = GameMain.GetPlayer();
            player.transform.position = m_CurrentLevelTransition.GetPlayerFeetSpawnPoint();
        }

        private LDtkComponentWorld GetChapeter(string chapterId)
        {
            var chapters = m_LdtkProject.Worlds;
            LDtkComponentWorld targetChapeter = null;

            foreach (var chapter in chapters)
            {
                if (chapter.Identifier == chapterId)
                {
                    targetChapeter = chapter;
                    break;
                }
            }

            return targetChapeter;
        }

        private LDtkComponentLevel GetLevel(string levelId)
        {
            var levels = m_CurrentWorld.Levels;
            LDtkComponentLevel targetLevel = null;

            foreach (var level in levels)
            {
                if (level.Identifier == levelId)
                {
                    targetLevel = level;
                    break;
                }
            }

            return targetLevel;
        }

        private LevelTransition GetLevelTransition(int levelSpawnPointIndex)
        {
            if (levelSpawnPointIndex < 0)
            {
                CLogger.LogError("Transition Id shouldn't below 0", LogTag.LevelManager);
            }

            List<LDtkComponentEntity> transitionsList = new();

            foreach (var layer in m_CurrentLevel.LayerInstances)
            {
                if (layer == null)
                    continue;

                foreach (LDtkComponentEntity entity in layer.EntityInstances)
                {
                    if (entity != null && entity.Identifier == LDtkIdentifiers.LevelTransition)
                        transitionsList.Add(entity);
                }
            }

            if (levelSpawnPointIndex >= transitionsList.Count)
            {
                CLogger.LogWarn(
                    "Spawn Id is bigger than level's spawn point count",
                    LogTag.LevelManager
                );
                levelSpawnPointIndex %= transitionsList.Count;
            }

            if (transitionsList.Count == 0)
                CLogger.LogError("No transition waw found in level", LogTag.LevelManager);

            return transitionsList.Count == 0
                ? null
                : transitionsList[levelSpawnPointIndex].GetComponent<LevelTransition>();
        }

        private LevelTransition m_CurrentLevelTransition = null;
        private LDtkComponentLevel m_CurrentLevel = null;
        private LDtkComponentWorld m_CurrentWorld = null;
        private LDtkComponentProject m_LdtkProject = null;
    }
}
