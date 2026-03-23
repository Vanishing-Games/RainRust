using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;

namespace GameMain.Editor
{
    public class LDtkTransitionProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            CLogger.LogInfo(
                $"Post process LDtk level: {root.name}",
                LogTag.LDtkTransitionProcessor
            );
            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            foreach (LDtkComponentLayer layer in level.LayerInstances)
            {
                if (layer == null || layer.EntityInstances == null)
                    continue;

                foreach (LDtkComponentEntity entity in layer.EntityInstances)
                {
                    if (entity != null && entity.Identifier == LDtkIdentifiers.LevelTransition)
                    {
                        var transitionGo = entity.gameObject;
                        var size = entity.Size;
                        transitionGo.transform.localScale = Vector3.one;

                        var collider = transitionGo.AddComponent<BoxCollider2D>();
                        collider.isTrigger = true;

                        // We assume the pivot is at the top-left corner, thus we need to adjust the position to center the collider
                        collider.offset = new Vector2(size.x / 2f, -size.y / 2f);
                        collider.size = size;

                        var levelTransition = transitionGo.AddComponent<LevelTransition>();
                        LDtkFields fields = transitionGo.GetComponent<LDtkFields>();
                        if (fields != null)
                        {
                            if (fields.TryGetInt("Index", out var index))
                            {
                                levelTransition.Index = index;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPostprocessProject(GameObject root)
        {
            CLogger.LogInfo(
                $"Post process LDtk project: {root.name}",
                LogTag.LDtkTransitionProcessor
            );

            // Find all LevelTransition components in the project to link them
            LevelTransition[] allTransitions = root.GetComponentsInChildren<LevelTransition>();

            foreach (var transition in allTransitions)
            {
                LDtkFields fields = transition.GetComponent<LDtkFields>();
                var targetRef = fields.GetEntityReference("Target");

                if (targetRef != null)
                {
                    LDtkIid targetIidComponent = LDtkIidComponentBank.GetByIid(targetRef.EntityIid);

                    if (targetIidComponent != null)
                    {
                        GameObject targetTransitionGo = targetIidComponent.gameObject;
                        var targetComp = targetTransitionGo.GetComponent<LevelTransition>();
                        if (targetComp != null)
                        {
                            transition.Target = targetComp;
                        }
                        else
                        {
                            CLogger.LogError(
                                $"关卡出入口: {transition.name} 有设置对应的目标 {targetTransitionGo.name}, 但目标对象上缺少 LevelTransition 组件",
                                LogTag.LDtkTransitionProcessor
                            );
                        }
                    }
                    else
                    {
                        CLogger.LogError(
                            $"关卡出入口: {transition.name} 有设置对应的目标 IID: {targetRef.EntityIid}, 但却无法在项目中获取目标对象",
                            LogTag.LDtkTransitionProcessor
                        );
                        transition.Target = null;
                    }
                }
                else
                {
                    CLogger.LogWarn(
                        $"关卡出入口: {transition.name} 没有设置对应的目标, 请确认这是否正确",
                        LogTag.LDtkTransitionProcessor
                    );
                    transition.Target = null;
                }
            }
        }
    }
}
