using System.Collections.Generic;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;


namespace GameMain.Editor
{
    public class LDtkSavePointProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 4;

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            CLogger.LogInfo(
                $"Post process LDtk level for SavePoints: {root.name}",
                LogTag.LDtkTransitionProcessor
            );
            
            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            if (level == null) return;

            var world = level.GetComponentInParent<LDtkComponentWorld>();
            string worldId = world != null ? world.Identifier : "World";
            string levelId = level.Identifier;

            int indexCounter = 0;

            foreach (LDtkComponentLayer layer in level.LayerInstances)
            {
                if (layer == null || layer.EntityInstances == null)
                    continue;

                foreach (LDtkComponentEntity entity in layer.EntityInstances)
                {
                    if (entity != null && entity.Identifier == "SavePoint")
                    {
                        var go = entity.gameObject;
                        var size = entity.Size;
                        go.transform.localScale = Vector3.one;
                        go.layer = LayerMask.NameToLayer("Ignore Raycast");

                        var collider = go.AddComponent<BoxCollider2D>();
                        collider.isTrigger = true;
                        collider.offset = new Vector2(size.x / 2f, -size.y / 2f);
                        collider.size = size;

                        var savePoint = go.AddComponent<SavePoint>();
                        savePoint.PointName = $"{worldId}_{levelId}_SavePoint{indexCounter}";
                        
                        LDtkFields fields = go.GetComponent<LDtkFields>();
                        if (fields != null)
                        {
                            if (fields.TryGetString("SavePointNickName", out var nickName))
                            {
                                savePoint.NickName = nickName;
                            }
                        }

                        indexCounter++;
                        EditorUtility.SetDirty(savePoint);
                    }
                }
            }
        }
    }
}
