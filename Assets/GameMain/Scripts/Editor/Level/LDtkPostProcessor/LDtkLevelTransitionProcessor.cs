using System.Collections.Generic;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public class LDtkTransitionProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 3;

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            CLogger.LogInfo(
                $"Post process LDtk level: {root.name}",
                LogTag.LDtkTransitionProcessor
            );
            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();

            int indexCounter = 0;

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
                        transitionGo.layer = LayerMask.NameToLayer("Ignore Raycast");

                        var collider = transitionGo.AddComponent<BoxCollider2D>();
                        collider.isTrigger = true;

                        // We assume the pivot is at the top-left corner, thus we need to adjust the position to center the collider
                        collider.offset = new Vector2(size.x / 2f, -size.y / 2f);
                        collider.size = size;

                        var levelTransition = transitionGo.AddComponent<LevelTransition>();
                        levelTransition.Index = indexCounter++;

                        LDtkFields fields = transitionGo.GetComponent<LDtkFields>();
                        if (fields != null)
                        {
                            var targetRef = fields.GetEntityReference("Target");
                            if (targetRef != null)
                                levelTransition.TargetIid = targetRef.EntityIid;
                        }

                        EditorUtility.SetDirty(levelTransition);
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

            LevelTransition[] allTransitions = root.GetComponentsInChildren<LevelTransition>();

            var iidToTransition = new Dictionary<string, LevelTransition>();
            foreach (var t in allTransitions)
            {
                var entity = t.GetComponent<LDtkComponentEntity>();
                if (entity != null)
                    iidToTransition[entity.Iid] = t;
            }

            foreach (var transition in allTransitions)
            {
                if (string.IsNullOrEmpty(transition.TargetIid))
                    continue;

                if (iidToTransition.TryGetValue(transition.TargetIid, out var targetComp))
                {
                    transition.Target = targetComp;
                    EditorUtility.SetDirty(transition);
                }
            }
        }
    }
}
