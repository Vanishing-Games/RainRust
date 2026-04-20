using Core;
using LDtkUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class InitInvokerCommands
    {
        /// <summary>
        /// 快速关卡测试命令 (自动根据位置寻找关卡)
        /// </summary>
        public class FastLevelTestCommand : ITriggerCommand
        {
            public FastLevelTestCommand(Vector3 testPosition)
            {
                m_TestPosition = testPosition;
            }

            public bool Execute()
            {
                CLogger.LogInfo(
                    $"[FastLevelTestCommand] Executing fast level test at {m_TestPosition}...",
                    LogTag.LevelManager
                );

                LDtkComponentLevel currentLevel = FastLevelTestHelper.FindLevelByPosition(
                    m_TestPosition
                );

                if (currentLevel == null)
                {
                    CLogger.LogError(
                        $"[FastLevelTestCommand] Can't find any LDtk level at position {m_TestPosition}",
                        LogTag.LevelManager
                    );
                    return false;
                }

                var world = currentLevel.GetComponentInParent<LDtkComponentWorld>(true);
                LevelManager.Instance.StartLevel(world.Identifier, currentLevel.Identifier);

                CLogger.LogInfo(
                    $"[FastLevelTestCommand] Started: Level={currentLevel.Identifier}",
                    LogTag.LevelManager
                );

                return true;
            }

            private readonly Vector3 m_TestPosition;
        }

        /// <summary>
        /// 快速关卡测试命令 (手动指定关卡)
        /// </summary>
        public class ManualFastLevelTestCommand : ITriggerCommand
        {
            public ManualFastLevelTestCommand(
                Vector3 testPosition,
                string chapterId,
                string levelId
            )
            {
                m_TestPosition = testPosition;
                m_ChapterId = chapterId;
                m_LevelId = levelId;
            }

            public ManualFastLevelTestCommand(Vector3 testPosition)
            {
                m_TestPosition = testPosition;
            }

            public bool Execute()
            {
                CLogger.LogInfo(
                    $"[ManualFastLevelTestCommand] Executing (Chapter={m_ChapterId}, Level={m_LevelId})...",
                    LogTag.LevelManager
                );

                LDtkComponentLevel targetLevel = null;

                if (!string.IsNullOrEmpty(m_LevelId))
                {
                    foreach (var level in LDtkComponentLevel.Levels)
                    {
                        if (level.Identifier != m_LevelId)
                            continue;
                        if (!string.IsNullOrEmpty(m_ChapterId) && level.Parent.Identifier != m_ChapterId)
                            continue;
                        targetLevel = level;
                        break;
                    }
                }

                if (targetLevel == null)
                    targetLevel = FastLevelTestHelper.FindLevelByPosition(m_TestPosition);

                if (targetLevel == null)
                {
                    CLogger.LogError(
                        "[ManualFastLevelTestCommand] Failed to identify level by ID or Position.",
                        LogTag.LevelManager
                    );
                    return false;
                }

                var world = targetLevel.GetComponentInParent<LDtkComponentWorld>(true);
                LevelManager.Instance.StartLevel(world.Identifier, targetLevel.Identifier);

                CLogger.LogInfo(
                    $"[ManualFastLevelTestCommand] Started: Level={targetLevel.Identifier}",
                    LogTag.LevelManager
                );

                return true;
            }

            private readonly Vector3 m_TestPosition;
            private readonly string m_ChapterId;
            private readonly string m_LevelId;
        }

        internal static class FastLevelTestHelper
        {
            public static LDtkComponentLevel FindLevelByPosition(Vector3 position)
            {
                foreach (var level in LDtkComponentLevel.Levels)
                {
                    if (level.BorderBounds.Contains(position))
                        return level;
                }
                return null;
            }
        }
    }
}
