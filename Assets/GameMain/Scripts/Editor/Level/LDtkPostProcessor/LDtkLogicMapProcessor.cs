using Core;
using Core.Extensions;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameMain.Editor
{
    public class LDtkLogicMapProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            CLogger.LogInfo($"Post process LDtk level: {root.name}", LogTag.LdtkLogicMapProcessor);

            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            if (level == null)
                return;

            foreach (LDtkComponentLayer layer in level.LayerInstances)
            {
                if (layer == null || layer.Identifier != LDtkIdentifiers.LogicMap)
                    continue;

                layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Wall"));
                layer.gameObject.SetTagRecursively("Wall");
            }
        }
    }
}
