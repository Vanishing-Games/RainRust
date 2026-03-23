using System;
using System.Collections.Generic;
using Core;
using LDtkUnity;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

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
        /// 进入指定关卡 (通过 Index)
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="levelId"></param>
        /// <param name="levelSpawnPointIndex"></param>
        public void StartLevel(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            InitLevelManager();
            SetUp(chapterId, levelId, levelSpawnPointIndex);
            StartLevelInternal();
        }

        /// <summary>
        /// 进入指定关卡 (直接通过 LevelTransition)
        /// </summary>
        /// <param name="transition"></param>
        public void StartLevel(LevelTransition transition)
        {
            if (transition == null)
            {
                CLogger.LogError("StartLevel: Transition is null", LogTag.LevelManager);
                return;
            }

            var level = transition.GetComponentInParent<LDtkComponentLevel>();
            if (level == null)
            {
                CLogger.LogError("StartLevel: Transition is not in a level", LogTag.LevelManager);
                return;
            }

            InitLevelManager();
            
            m_CurrentLevel = level;
            m_CurrentLevelTransition = transition;
            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>();
            
            StartLevelInternal();
        }

        private void StartLevelInternal()
        {
            SetUpPlayer();
            ActivateRoom(m_CurrentLevel);
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
                return null;
            }

            List<LevelTransition> transitionsList = new();

            foreach (var layer in m_CurrentLevel.LayerInstances)
            {
                if (layer == null)
                    continue;

                foreach (LDtkComponentEntity entity in layer.EntityInstances)
                {
                    if (entity != null && entity.Identifier == LDtkIdentifiers.LevelTransition)
                    {
                        var transition = entity.GetComponent<LevelTransition>();
                        if (transition != null)
                        {
                            // Priority 1: Match by the explicit Index property
                            if (transition.Index.HasValue && transition.Index.Value == levelSpawnPointIndex)
                            {
                                return transition;
                            }
                            transitionsList.Add(transition);
                        }
                    }
                }
            }

            // Priority 2: Fallback to list order if no exact Index match was found
            if (transitionsList.Count > 0)
            {
                if (levelSpawnPointIndex >= transitionsList.Count)
                {
                    CLogger.LogWarn(
                        $"Spawn Index {levelSpawnPointIndex} not found by ID, and is bigger than level's transition count. Falling back to modulo.",
                        LogTag.LevelManager
                    );
                    levelSpawnPointIndex %= transitionsList.Count;
                }
                return transitionsList[levelSpawnPointIndex];
            }

            CLogger.LogError($"No transition found in level {m_CurrentLevel.Identifier}", LogTag.LevelManager);
            return null;
        }

        /// <summary>
        /// 切换到指定关卡 (用于无缝切换)
        /// </summary>
        /// <param name="targetTransition"></param>
        public void SwitchLevel(LevelTransition targetTransition)
        {
            var level = targetTransition.GetComponentInParent<LDtkComponentLevel>();
            if (level == null)
            {
                CLogger.LogError("LevelTransition is not in a level", LogTag.LevelManager);
                return;
            }

            if (m_CurrentLevel == level)
                return;

            m_CurrentLevel = level;
            m_CurrentLevelTransition = targetTransition;
            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>();

            CLogger.LogInfo($"Switched to Level: {level.Identifier}", LogTag.LevelManager);
            
            ActivateRoom(level);
        }

        private void ActivateRoom(LDtkComponentLevel level)
        {
            if (m_CurrentRoom != null)
            {
                m_CurrentRoom.SetActive(false);
            }

            m_CurrentRoom = level.GetComponent<LevelRoom>();
            if (m_CurrentRoom != null)
            {
                m_CurrentRoom.SetActive(true);
                
                // If it's follow mode, ensure the player is being followed
                if (m_CurrentRoom.CameraMode == CameraMode.Follow && m_CurrentRoom.VirtualCamera != null)
                {
                    var player = GameMain.GetPlayer();
                    if (player != null)
                    {
                        m_CurrentRoom.VirtualCamera.Follow = player.transform;
                    }
                }
            }
        }

        internal LevelTransition GetCurrentTransition()
        {
            return m_CurrentLevelTransition;
        }

        private LevelRoom m_CurrentRoom = null;
        private LevelTransition m_CurrentLevelTransition = null;
        private LDtkComponentLevel m_CurrentLevel = null;
        private LDtkComponentWorld m_CurrentWorld = null;
        private LDtkComponentProject m_LdtkProject = null;
    }
}
