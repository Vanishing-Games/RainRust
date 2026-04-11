using System.Collections.Generic;
using System.Linq;
using Core;
using LDtkUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class InitInvokerCommands
    {
        /// <summary>
        /// 快速关卡测试命令 (自动寻找最近的 Transition)
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
                    $"[FastLevelTestCommand] Executing fast level test at {m_TestPosition} (Auto nearest)...",
                    LogTag.LevelManager
                );

                // 1. 查找关卡
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

                // 2. 查找最近的 Transition
                LevelTransition nearestTransition = FastLevelTestHelper.FindNearestTransition(
                    currentLevel,
                    m_TestPosition
                );

                if (nearestTransition == null)
                {
                    CLogger.LogError(
                        $"[FastLevelTestCommand] No LevelTransition found in level {currentLevel.Identifier}",
                        LogTag.LevelManager
                    );
                    return false;
                }

                // 3. 启动
                LevelManager.Instance.StartLevel(nearestTransition);

                CLogger.LogInfo(
                    $"[FastLevelTestCommand] Started: Level={currentLevel.Identifier}, Nearest={nearestTransition.name} at {nearestTransition.transform.position}",
                    LogTag.LevelManager
                );

                return true;
            }

            private readonly Vector3 m_TestPosition;
        }

        /// <summary>
        /// 快速关卡测试命令 (手动指定关卡或 Index)
        /// </summary>
        public class ManualFastLevelTestCommand : ITriggerCommand
        {
            public ManualFastLevelTestCommand(
                Vector3 testPosition,
                string chapterId,
                string levelId,
                int index
            )
            {
                m_TestPosition = testPosition;
                m_ChapterId = chapterId;
                m_LevelId = levelId;
                m_Index = index;
            }

            public ManualFastLevelTestCommand(Vector3 testPosition, int index)
            {
                m_TestPosition = testPosition;
                m_Index = index;
            }

            public bool Execute()
            {
                CLogger.LogInfo(
                    $"[ManualFastLevelTestCommand] Executing (Chapter={m_ChapterId}, Level={m_LevelId}, Index={m_Index})...",
                    LogTag.LevelManager
                );

                // 1. 识别目标 Level (优先使用 ID)
                LDtkComponentLevel targetLevel = null;
                if (!string.IsNullOrEmpty(m_LevelId))
                {
                    targetLevel = LDtkComponentLevel.Levels.FirstOrDefault(l =>
                        l.Identifier == m_LevelId
                        && (string.IsNullOrEmpty(m_ChapterId) || l.Parent.Identifier == m_ChapterId)
                    );
                }

                // Fallback: 使用位置识别
                if (targetLevel == null)
                {
                    targetLevel = FastLevelTestHelper.FindLevelByPosition(m_TestPosition);
                }

                if (targetLevel == null)
                {
                    CLogger.LogError(
                        "[ManualFastLevelTestCommand] Failed to identify level by ID or Position.",
                        LogTag.LevelManager
                    );
                    return false;
                }

                // 2. 识别目标 Transition
                LevelTransition targetTransition = null;

                // 必须匹配 LevelTransition.Index 属性
                var transitions = targetLevel.GetComponentsInChildren<LevelTransition>(true);
                targetTransition = transitions.FirstOrDefault(t => t.Index == m_Index);

                // 如果还是没找到，或者没指定有效 Index，寻找最近的
                if (targetTransition == null)
                {
                    CLogger.LogWarn(
                        $"[ManualFastLevelTestCommand] Transition with Index {m_Index} not found in level {targetLevel.Identifier}, falling back to nearest transition.",
                        LogTag.LevelManager
                    );
                    targetTransition = FastLevelTestHelper.FindNearestTransition(
                        targetLevel,
                        m_TestPosition
                    );
                }

                if (targetTransition == null)
                {
                    CLogger.LogError(
                        $"[ManualFastLevelTestCommand] No transition found in level {targetLevel.Identifier}",
                        LogTag.LevelManager
                    );
                    return false;
                }

                // 3. 启动
                LevelManager.Instance.StartLevel(targetTransition);

                CLogger.LogInfo(
                    $"[ManualFastLevelTestCommand] Started: Level={targetLevel.Identifier}, Transition={targetTransition.name}",
                    LogTag.LevelManager
                );

                return true;
            }

            private readonly Vector3 m_TestPosition;
            private readonly string m_ChapterId;
            private readonly string m_LevelId;
            private readonly int m_Index;
        }

        /// <summary>
        /// 内部工具类
        /// </summary>
        internal static class FastLevelTestHelper
        {
            public static LDtkComponentLevel FindLevelByPosition(Vector3 position)
            {
                foreach (var level in LDtkComponentLevel.Levels)
                {
                    if (level.BorderBounds.Contains(position))
                    {
                        return level;
                    }
                }
                return null;
            }

            public static LevelTransition FindNearestTransition(
                LDtkComponentLevel level,
                Vector3 position
            )
            {
                var transitions = level.GetComponentsInChildren<LevelTransition>(true);
                if (transitions.Length == 0)
                    return null;

                LevelTransition nearest = null;
                float minDistance = float.MaxValue;

                foreach (var t in transitions)
                {
                    Vector3 targetPos = t.transform.position;

                    // 如果有 Collider，使用 Collider 的中心点作为计算参考
                    var box = t.GetComponent<BoxCollider2D>();
                    if (box != null)
                    {
                        targetPos = t.transform.TransformPoint(box.offset);
                    }

                    float dist = Vector2.Distance(position, targetPos);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = t;
                    }
                }
                return nearest;
            }
        }
    }
}
