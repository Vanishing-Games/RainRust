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
            Core.Logger.LogInfo(
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

                        var collider = transitionGo.AddComponent<BoxCollider2D>();
                        collider.isTrigger = true;

                        // We assume the pivot is at the top-left corner, thus we need to adjust the position to center the collider
                        collider.offset = new Vector2(size.x / 2f, -size.y / 2f);
                        collider.size = size;
                        transitionGo.AddComponent<LevelTransition>();
                    }
                }
            }
        }
    }
}
