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

                        var transition = transitionGo.AddComponent<LevelTransition>();
                        LDtkFields fields = entity.GetComponent<LDtkFields>();

                        var targetRef = fields.GetEntityReference("Target");

                        if (targetRef != null)
                        {
                            LDtkIid targetIidComponent = LDtkIidComponentBank.GetByIid(
                                targetRef.EntityIid
                            );

                            if (targetIidComponent != null)
                            {
                                GameObject targetTransitionGo = targetIidComponent.gameObject;
                                transition.Target =
                                    targetTransitionGo.GetComponent<LevelTransition>();
                            }
                            else
                            {
                                CLogger.LogError(
                                    "关卡出入口: "
                                        + entity.name
                                        + " 有设置对应的目标, 但却无法获取Go",
                                    LogTag.LDtkTransitionProcessor
                                );
                            }
                        }
                        else
                        {
                            CLogger.LogWarn(
                                "关卡出入口: "
                                    + entity.name
                                    + " 没有设置对应的目标, 请确认这是否正确\n Fields如下:"
                                    + fields,
                                LogTag.LDtkTransitionProcessor
                            );
                        }
                    }
                }
            }
        }
    }
}
