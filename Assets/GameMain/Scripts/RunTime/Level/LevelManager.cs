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
        /// 开启一个关卡（通过 SavePoint）
        /// </summary>
        /// <param name="savePointName">SavePoint的NickName或PointName</param>
        public void StartLevel(string savePointName)
        {
            InitLevelManager();
            SetUpWithSavePoint(savePointName);
            StartLevelInternal();
        }

        /// <summary>
        /// 开启一个关卡 (通过 Index, 保留兼容并添加 Clamp 逻辑)
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="levelId"></param>
        /// <param name="levelSpawnPointIndex"></param>
        public void StartLevel(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            InitLevelManager();
            SetUp(chapterId, levelId, levelSpawnPointIndex);
            
            var player = GameMain.GetPlayer();
            if (player != null && m_CurrentLevelTransition != null)
            {
                player.transform.position = m_CurrentLevelTransition.GetPlayerFeetSpawnPoint();
            }

            StartLevelInternal();
        }

        /// <summary>
        /// 进入指定关卡 (通过 LevelTransition)
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

            var player = GameMain.GetPlayer();
            if (player != null && m_CurrentLevelTransition != null)
            {
                player.transform.position = m_CurrentLevelTransition.GetPlayerFeetSpawnPoint();
            }

            StartLevelInternal();
        }

        internal int GetCurrentMaxPriority() => ++m_CurrentMaxPriority;

        public LDtkComponentLevel CurrentLevel => m_CurrentLevel;
        public LevelTransition CurrentTransition => m_CurrentLevelTransition;

        public void ClearCurrentTransition()
        {
            m_CurrentLevelTransition = null;
        }

        private int m_LastSwitchFrame = -1;

        internal LevelTransition GetCurrentTransition()
        {
            return m_CurrentLevelTransition;
        }

        private void StartLevelInternal()
        {
            m_LastSwitchFrame = Time.frameCount;
            ActivateRoom(m_CurrentLevel);

#if UNITY_EDITOR
            UpdateDebugUI();
#endif
        }

#if UNITY_EDITOR
        private void UpdateDebugUI()
        {
            DebugUIManager.Log(
                "Chapter",
                m_CurrentWorld != null ? m_CurrentWorld.Identifier : "None"
            );
            DebugUIManager.Log(
                "Level",
                m_CurrentLevel != null ? m_CurrentLevel.Identifier : "None"
            );
            DebugUIManager.Log(
                "Transition",
                m_CurrentLevelTransition != null ? m_CurrentLevelTransition.name : "None"
            );
            DebugUIManager.Log(
                "Camera Mode",
                m_CurrentRoom != null ? m_CurrentRoom.CameraMode : "None"
            );
        }
#endif

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

        private void SetUpWithSavePoint(string savePointName)
        {
            var allSavePoints = FindObjectsByType<SavePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            SavePoint targetSavePoint = null;

            foreach (var sp in allSavePoints)
            {
                if (sp.NickName == savePointName)
                {
                    targetSavePoint = sp;
                    break;
                }
            }

            if (targetSavePoint == null)
            {
                foreach (var sp in allSavePoints)
                {
                    if (sp.PointName == savePointName)
                    {
                        targetSavePoint = sp;
                        break;
                    }
                }
            }

            if (targetSavePoint == null)
            {
                CLogger.LogError($"Can't Find SavePoint: {savePointName}", LogTag.LevelManager);
                return;
            }

            var level = targetSavePoint.GetComponentInParent<LDtkComponentLevel>();
            if (level == null)
            {
                CLogger.LogError($"SavePoint {savePointName} is not in a level", LogTag.LevelManager);
                return;
            }

            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>();
            m_CurrentLevel = level;
            m_CurrentLevelTransition = null;

            var player = GameMain.GetPlayer();
            if (player != null)
            {
                player.transform.position = targetSavePoint.transform.position;
            }
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
                            transitionsList.Add(transition);
                        }
                    }
                }
            }

            if (transitionsList.Count > 0)
            {
                transitionsList.Sort((a, b) => a.Index.CompareTo(b.Index));
                
                int clampedIndex = Mathf.Clamp(levelSpawnPointIndex, 0, transitionsList.Count - 1);
                if (levelSpawnPointIndex != clampedIndex)
                {
                    CLogger.LogWarn(
                        $"Spawn Index {levelSpawnPointIndex} is out of bounds for level {m_CurrentLevel.Identifier} (max {transitionsList.Count - 1}). Clamped to {clampedIndex}.",
                        LogTag.LevelManager
                    );
                }
                return transitionsList[clampedIndex];
            }

            CLogger.LogError(
                $"No transition found in level {m_CurrentLevel.Identifier}",
                LogTag.LevelManager
            );
            return null;
        }

        /// <summary>
        /// 切换到指定关卡 (仅进行 room 的切换，不处理其他初始化)
        /// </summary>
        /// <param name="targetTransition"></param>
        public void SwitchLevel(LevelTransition targetTransition)
        {
            if (Time.frameCount == m_LastSwitchFrame)
                return;

            var level = targetTransition.GetComponentInParent<LDtkComponentLevel>();
            if (level == null)
            {
                CLogger.LogError("LevelTransition is not in a level", LogTag.LevelManager);
                return;
            }

            if (m_CurrentLevel == level)
                return;

            var posDiff = GetTransitionPositionDiff(targetTransition);

            m_LastSwitchFrame = Time.frameCount;
            m_CurrentLevel = level;
            m_CurrentLevelTransition = targetTransition;
            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>();

            CLogger.LogInfo($"Switched to Level: {level.Identifier}", LogTag.LevelManager);

            ActivateRoom(level);

            if (posDiff.HasValue)
            {
                var posDiffValue = posDiff.Value;
                MessageBroker.Global.Publish<LevelSwitchEvent>(
                    new(
                        posDiffValue.x == 0
                            ? LevelSwitchEvent.LevelSwitchDirection.Vertical
                            : LevelSwitchEvent.LevelSwitchDirection.Horizontal
                    )
                );
            }
            else
            {
                CLogger.LogError(
                    "SENDING LevelSwitchEvent FAILED\n"
                        + "Something went wrong when calculating level transition difference, maybe a null ref issue. info follows:"
                        + " \nCurLevel: "
                        + m_CurrentLevelTransition
                        + " \nTargetLevel: "
                        + targetTransition,
                    LogTag.LevelManager,
                    LogTag.Event
                );
            }

#if UNITY_EDITOR
            UpdateDebugUI();
#endif
        }

        private void ActivateRoom(LDtkComponentLevel level)
        {
            if (m_CurrentRoom != null)
            {
                // m_CurrentRoom.SetActive(false);
            }

            m_CurrentRoom = level.GetComponent<LevelRoom>();
            if (m_CurrentRoom != null && m_CurrentRoom.VirtualCamera != null)
            {
                m_CurrentRoom.Activate();
            }
            else
            {
                CLogger.LogError(
                    "There's no LevelRoom Component in level: "
                        + level.name
                        + "\nOr There's no virtual camera setted",
                    LogTag.LevelManager
                );
            }
        }

        private Vector3? GetTransitionPositionDiff(LevelTransition targetTransition)
        {
            if (targetTransition == null)
                return null;

            if (m_CurrentLevelTransition == null)
                return null;

            var tarPos = targetTransition.transform.position;
            var curPos = m_CurrentLevelTransition.transform.position;
            return tarPos - curPos;
        }

        private int m_CurrentMaxPriority = 0;
        private LevelRoom m_CurrentRoom = null;
        private LevelTransition m_CurrentLevelTransition = null;
        private LDtkComponentLevel m_CurrentLevel = null;
        private LDtkComponentWorld m_CurrentWorld = null;
        private LDtkComponentProject m_LdtkProject = null;
    }
}
