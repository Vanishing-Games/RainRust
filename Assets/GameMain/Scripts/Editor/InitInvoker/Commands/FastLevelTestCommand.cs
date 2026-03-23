using System.Collections.Generic;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using R3;
using UnityEngine;

namespace GameMain.Editor
{
    /// <summary>
    /// 快速关卡测试命令
    /// 逻辑：根据给定位置找到对应的 LDtk Level 和最近的出生点，然后启动关卡
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
            CLogger.LogInfo("[FastLevelTestCommand] Executing fast level test...", LogTag.LevelManager);


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

            // 查找最近的 LevelTransition 并获取其 Index
            List<LevelTransition> transitions = new();
            foreach (var layer in currentLevel.LayerInstances)
            {
                if(layer == null)
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

            int nearestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < transitions.Count; i++)
            {
                float dist = Vector3.Distance(m_TestPosition, transitions[i].transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestIndex = i;
                }
            }

            // 启动关卡
            LevelManager.Instance.StartLevel(
                currentLevel.Parent.Identifier,
                currentLevel.Identifier,
                nearestIndex
            );

            CLogger.LogInfo(
                $"[FastLevelTestCommand] Fast Level Test Started: Level={currentLevel.Identifier}, Index={nearestIndex}",
                LogTag.LevelManager
            );

            return true;
        }
    }
}
