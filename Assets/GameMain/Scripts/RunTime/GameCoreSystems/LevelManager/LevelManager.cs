using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using LDtkUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelManager : CoreModuleManagerBase<LevelManager>, ICoreModuleSystem
    {
        public string SystemName => "LevelManager";
        public Type[] Dependencies => new[] { typeof(VgSceneManager), typeof(VgSaveSystem) };

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnLoadComplete(async ctx =>
            {
                if (ctx.Destination == GameFlowState.InLevel)
                {
                    if (!string.IsNullOrEmpty(ctx.SavePointName))
                    {
                        StartLevel(ctx.SavePointName);
                    }
                    else if (ctx.IsStandalone)
                    {
                        await SetupExistingLevel(ctx);
                    }
                    else
                    {
                        StartLevel(ctx.ChapterId, ctx.LevelId);
                    }
                }
            });

            registry.OnInLevelExit(async () =>
            {
                EndLevel();
                await UniTask.CompletedTask;
            });
        }

        private async UniTask SetupExistingLevel(LoadContext ctx)
        {
            InitLevelManager();
            if (!string.IsNullOrEmpty(ctx.ChapterId) && !string.IsNullOrEmpty(ctx.LevelId))
            {
                SetUp(ctx.ChapterId, ctx.LevelId);
            }
            else
            {
                var levels = FindObjectsByType<LDtkComponentLevel>(FindObjectsSortMode.None);
                if (levels.Length > 0)
                {
                    m_CurrentLevel = levels[0];
                    m_CurrentWorld = m_CurrentLevel.GetComponentInParent<LDtkComponentWorld>(true);
                    ActivateRoom(m_CurrentLevel);
                }
            }
            StartLevelInternal();
            await UniTask.CompletedTask;
        }

        public void StartLevel(string savePointName)
        {
            InitLevelManager();
            if (!SetUpWithSavePoint(savePointName))
                return;
            StartLevelInternal();
        }

        public void StartLevel(string chapterId, string levelId)
        {
            InitLevelManager();
            if (!SetUp(chapterId, levelId))
                return;

            var player = GameMain.GetPlayer();
            if (player != null)
            {
                var center = m_CurrentLevel.BorderBounds.center;
                player.transform.position = new Vector3(center.x, center.y, player.transform.position.z);
            }

            StartLevelInternal();
        }

        public LDtkComponentLevel CurrentLevel => m_CurrentLevel;

        internal int GetCurrentMaxPriority() => ++m_CurrentMaxPriority;

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
                BuildSpatialIndex();
            }
        }

        private void BuildSpatialIndex()
        {
            m_WorldLevelsMap.Clear();
            m_LevelNeighborMap.Clear();

            foreach (var world in m_LdtkProject.Worlds)
                m_WorldLevelsMap[world] = world.GetComponentsInChildren<LDtkComponentLevel>(true);

            foreach (var world in m_LdtkProject.Worlds)
            {
                foreach (var level in world.GetComponentsInChildren<LDtkComponentLevel>(true))
                {
                    var room = level.GetComponent<LevelRoom>();
                    if (room == null)
                        continue;
                    var neighbors = new HashSet<LDtkComponentLevel>();
                    foreach (var neighborRoom in room.Neighbors)
                    {
                        var neighborLevel = neighborRoom.GetComponent<LDtkComponentLevel>();
                        if (neighborLevel != null)
                            neighbors.Add(neighborLevel);
                    }
                    m_LevelNeighborMap[level] = neighbors;
                }
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

            var player = GameMain.GetPlayer();
            if (player != null)
            {
                player.transform.position = targetSavePoint.transform.position;
            }

            return true;
        }

        private bool SetUp(string chapterId, string levelId)
        {
            m_CurrentWorld = GetChapter(chapterId);
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

            return true;
        }

        private LDtkComponentWorld GetChapter(string chapterId)
        {
            var chapters = m_LdtkProject.Worlds;
            LDtkComponentWorld targetChapter = null;

            foreach (var chapter in chapters)
            {
                if (chapter.Identifier == chapterId)
                {
                    targetChapter = chapter;
                    break;
                }
            }

            return targetChapter;
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

        public void SwitchLevel(LDtkComponentLevel level)
        {
            if (Time.frameCount == m_LastSwitchFrame)
                return;

            if (level == null)
            {
                CLogger.LogError("SwitchLevel: level is null", LogTag.LevelManager);
                return;
            }

            if (m_CurrentLevel == level)
                return;

            var posDiff = level.BorderBounds.center - m_CurrentLevel.BorderBounds.center;

            m_LastSwitchFrame = Time.frameCount;
            m_CurrentLevel = level;
            m_CurrentWorld = level.GetComponentInParent<LDtkComponentWorld>();

            CLogger.LogInfo($"Switched to Level: {level.Identifier}", LogTag.LevelManager);

            ActivateRoom(level);
            CullRooms();

            var direction = Mathf.Abs(posDiff.x) >= Mathf.Abs(posDiff.y)
                ? LevelManagerEvents.LevelSwitchDirection.Horizontal
                : LevelManagerEvents.LevelSwitchDirection.Vertical;

            MessageBroker.Global.Publish<LevelManagerEvents.LevelManagerLevelSwitchedEvent>(
                new(direction)
            );

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
            return;
        }

        private int m_CurrentMaxPriority = 0;
        private int m_LastSwitchFrame = -1;
        private LevelRoom m_CurrentRoom = null;
        private LDtkComponentLevel m_CurrentLevel = null;
        private LDtkComponentWorld m_CurrentWorld = null;
        private LDtkComponentProject m_LdtkProject = null;
        private Dictionary<LDtkComponentWorld, LDtkComponentLevel[]> m_WorldLevelsMap = new();
        private Dictionary<LDtkComponentLevel, HashSet<LDtkComponentLevel>> m_LevelNeighborMap =
            new();
    }
}
