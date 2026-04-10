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
        /// 开启一个关卡（通过 SavePoint）
        /// </summary>
        /// <param name="savePointName">SavePoint的NickName或PointName</param>
        public void StartLevel(string savePointName)
        {
            InitLevelManager();
            if (!SetUpWithSavePoint(savePointName))
                return;
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
            if (!SetUp(chapterId, levelId, levelSpawnPointIndex))
                return;

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

            var level = transition.GetComponentInParent<LDtkComponentLevel>(true);
            if (level == null)
            {
                CLogger.LogError("StartLevel: Transition is not in a level", LogTag.LevelManager);
                return;
            }

            InitLevelManager();

            m_CurrentLevel = level;
            m_CurrentLevelTransition = transition;
            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>(true);

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
            EndLevel();
            m_LastSwitchFrame = Time.frameCount;

            m_CurrentWorld.gameObject.SetActive(true);
            foreach (var world in m_LdtkProject.Worlds)
            {
                if (world != m_CurrentWorld)
                    world.gameObject.SetActive(false);
            }

            MessageBroker.Global.Publish<LevelManagerEvents.LevelManagerPreEnterChapterEvent>(
                new(m_CurrentWorld.Identifier)
            );

            ActivateRoom(m_CurrentLevel);
            CullRooms();

            MessageBroker.Global.Publish<LevelManagerEvents.LevelManagerPostEnterChapterEvent>(
                new(m_CurrentWorld.Identifier)
            );

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

        public void EndLevel()
        {
            if (m_CurrentWorld == null)
                return;

            MessageBroker.Global.Publish<LevelManagerEvents.LevelManagerPreExitChapterEvent>(
                new(m_CurrentWorld.Identifier)
            );

            MessageBroker.Global.Publish<LevelManagerEvents.LevelManagerPostExitChapterEvent>(
                new(m_CurrentWorld.Identifier)
            );
        }

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
            {
                m_LdtkProject = ldtkProjects[0];
                LinkUnresolvedTransitions();
                BuildSpatialIndex();
            }
        }

        private void LinkUnresolvedTransitions()
        {
            var allEntities = FindObjectsByType<LDtkComponentEntity>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            var iidToTransition = new Dictionary<string, LevelTransition>();
            foreach (var entity in allEntities)
            {
                if (entity.TryGetComponent<LevelTransition>(out var t))
                    iidToTransition[entity.Iid] = t;
            }

            var allTransitions = FindObjectsByType<LevelTransition>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            foreach (var transition in allTransitions)
            {
                if (transition.Target != null || string.IsNullOrEmpty(transition.TargetIid))
                    continue;

                if (iidToTransition.TryGetValue(transition.TargetIid, out var target))
                    transition.Target = target;
                else
                    CLogger.LogError(
                        $"运行时无法解析关卡出入口 {transition.name} 的目标 IID: {transition.TargetIid}",
                        LogTag.LevelManager
                    );
            }
        }

        private void BuildSpatialIndex()
        {
            m_WorldLevelsMap.Clear();
            m_LevelNeighborMap.Clear();

            foreach (var world in m_LdtkProject.Worlds)
                m_WorldLevelsMap[world] = world.GetComponentsInChildren<LDtkComponentLevel>(true);

            var allTransitions = FindObjectsByType<LevelTransition>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            foreach (var transition in allTransitions)
            {
                if (transition.Target == null)
                    continue;

                var levelA = transition.GetComponentInParent<LDtkComponentLevel>(true);
                var levelB = transition.Target.GetComponentInParent<LDtkComponentLevel>(true);

                if (levelA == null || levelB == null || levelA == levelB)
                    continue;

                if (!m_LevelNeighborMap.TryGetValue(levelA, out var neighborsA))
                    m_LevelNeighborMap[levelA] = neighborsA = new HashSet<LDtkComponentLevel>();
                neighborsA.Add(levelB);

                if (!m_LevelNeighborMap.TryGetValue(levelB, out var neighborsB))
                    m_LevelNeighborMap[levelB] = neighborsB = new HashSet<LDtkComponentLevel>();
                neighborsB.Add(levelA);
            }
        }

        private bool SetUpWithSavePoint(string savePointName)
        {
            var allSavePoints = FindObjectsByType<SavePoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
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
                return false;
            }

            var level = targetSavePoint.GetComponentInParent<LDtkComponentLevel>(true);
            if (level == null)
            {
                CLogger.LogError(
                    $"SavePoint {savePointName} is not in a level",
                    LogTag.LevelManager
                );
                return false;
            }

            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>(true);
            m_CurrentLevel = level;
            m_CurrentLevelTransition = null;

            var player = GameMain.GetPlayer();
            if (player != null)
            {
                player.transform.position = targetSavePoint.transform.position;
            }

            return true;
        }

        private bool SetUp(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            m_CurrentWorld = GetChapeter(chapterId);
            if (m_CurrentWorld == null)
            {
                CLogger.LogError("Can't Find Chapter: " + chapterId, LogTag.LevelManager);
                return false;
            }

            m_CurrentLevel = GetLevel(levelId);
            if (m_CurrentLevel == null)
            {
                CLogger.LogError("Can't Find Level: " + levelId, LogTag.LevelManager);
                return false;
            }

            m_CurrentLevelTransition = GetLevelTransition(levelSpawnPointIndex);
            if (m_CurrentLevelTransition == null)
            {
                CLogger.LogError(
                    "Can't Find LevelTransition: " + levelSpawnPointIndex,
                    LogTag.LevelManager
                );
                return false;
            }

            return true;
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
                        if (entity.TryGetComponent<LevelTransition>(out var transition))
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
            CullRooms();

            if (posDiff.HasValue)
            {
                var posDiffValue = posDiff.Value;
                MessageBroker.Global.Publish<LevelManagerEvents.LevelManagerLevelSwitchedEvent>(
                    new(
                        posDiffValue.x == 0
                            ? LevelManagerEvents.LevelSwitchDirection.Vertical
                            : LevelManagerEvents.LevelSwitchDirection.Horizontal
                    )
                );
            }
            else
            {
                CLogger.LogError(
                    "SENDING LevelSwitchedEvent FAILED\n"
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
                m_CurrentRoom.DeActivate();

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

        private void CullRooms()
        {
            return; // 开启这个函数反而会导致切换房间时剧烈卡顿

            if (m_CurrentWorld == null || m_CurrentLevel == null)
                return;

            if (!m_WorldLevelsMap.TryGetValue(m_CurrentWorld, out var allLevels))
                return;

            m_LevelNeighborMap.TryGetValue(m_CurrentLevel, out var neighborLevels);

            foreach (var level in allLevels)
            {
                bool shouldBeActive =
                    level == m_CurrentLevel
                    || (neighborLevels != null && neighborLevels.Contains(level));
                if (level.gameObject.activeSelf != shouldBeActive)
                    level.gameObject.SetActive(shouldBeActive);
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
        private Dictionary<LDtkComponentWorld, LDtkComponentLevel[]> m_WorldLevelsMap = new();
        private Dictionary<LDtkComponentLevel, HashSet<LDtkComponentLevel>> m_LevelNeighborMap =
            new();
    }
}
