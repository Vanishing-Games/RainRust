using System.Collections.Generic;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using UnityEngine;

namespace GameMain.Editor
{
    /// <summary>
    /// 快速关卡测试命令 (自动寻找最近的 Transition)
    /// </summary>
    public class FastLevelTestCommand : ITriggerCommand
    {
        private readonly Vector3 m_TestPosition;

        public FastLevelTestCommand(Vector3 testPosition)
        {
            m_TestPosition = testPosition;
        }

        public bool Execute()
        {
            CLogger.LogInfo(
                "[FastLevelTestCommand] Executing fast level test (Auto nearest)...",
                LogTag.LevelManager
            );

            // 查找当前位置所在的 Level
            LDtkComponentLevel currentLevel = null;
            foreach (var level in LDtkComponentLevel.Levels)
            {
                if (level.BorderBounds.Contains(m_TestPosition))
                {
                    currentLevel = level;
                    break;
                }
            }

            if (currentLevel == null)
            {
                CLogger.LogError(
                    $"[FastLevelTestCommand] Can't find any LDtk level at position {m_TestPosition}",
                    LogTag.LevelManager
                );
                return false;
            }

            // 查找最近的 LevelTransition
            List<LevelTransition> transitions = new();
            foreach (var layer in currentLevel.LayerInstances)
            {
                if (layer == null)
                    continue;

                foreach (var entity in layer.EntityInstances)
                {
                    if (entity != null && entity.Identifier == LDtkIdentifiers.LevelTransition)
                    {
                        var transition = entity.GetComponent<LevelTransition>();
                        if (transition != null)
                        {
                            transitions.Add(transition);
                        }
                    }
                }
            }

            if (transitions.Count == 0)
            {
                CLogger.LogError(
                    $"[FastLevelTestCommand] No LevelTransition found in level {currentLevel.Identifier}",
                    LogTag.LevelManager
                );
                return false;
            }

            LevelTransition nearestTransition = null;
            float minDistance = float.MaxValue;

            foreach (var t in transitions)
            {
                float dist = Vector3.Distance(m_TestPosition, t.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestTransition = t;
                }
            }

            // 启动关卡 (使用新的直接接受 LevelTransition 的重载)
            LevelManager.Instance.StartLevel(nearestTransition);

            CLogger.LogInfo(
                $"[FastLevelTestCommand] Fast Level Test Started: Level={currentLevel.Identifier}, Nearest Transition={nearestTransition.name}",
                LogTag.LevelManager
            );

            return true;
        }
    }

    /// <summary>
    /// 快速关卡测试命令 (手动指定 Index)
    /// </summary>
    public class ManualFastLevelTestCommand : ITriggerCommand
    {
        private readonly Vector3 m_TestPosition;
        private readonly int m_Index;

        public ManualFastLevelTestCommand(Vector3 testPosition, int index)
        {
            m_TestPosition = testPosition;
            m_Index = index;
        }

        public bool Execute()
        {
            CLogger.LogInfo(
                $"[ManualFastLevelTestCommand] Executing fast level test with Index {m_Index}...",
                LogTag.LevelManager
            );

            // 查找当前位置所在的 Level
            LDtkComponentLevel currentLevel = null;
            foreach (var level in LDtkComponentLevel.Levels)
            {
                if (level.BorderBounds.Contains(m_TestPosition))
                {
                    currentLevel = level;
                    break;
                }
            }

            if (currentLevel == null)
            {
                CLogger.LogError(
                    $"[ManualFastLevelTestCommand] Can't find any LDtk level at position {m_TestPosition}",
                    LogTag.LevelManager
                );
                return false;
            }

            // 启动关卡 (使用接受 Index 的重载)
            LevelManager.Instance.StartLevel(
                currentLevel.Parent.Identifier,
                currentLevel.Identifier,
                m_Index
            );

            CLogger.LogInfo(
                $"[ManualFastLevelTestCommand] Fast Level Test Started: Level={currentLevel.Identifier}, Index={m_Index}",
                LogTag.LevelManager
            );

            return true;
        }
    }
}
