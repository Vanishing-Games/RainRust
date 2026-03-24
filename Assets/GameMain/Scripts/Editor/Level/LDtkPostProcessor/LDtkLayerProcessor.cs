using Core;
using Core.Extensions;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;

namespace GameMain.Editor
{
    public class LDtkLayerProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 0;

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            CLogger.LogInfo($"Post process LDtk level: {root.name}", LogTag.LdtkLogicMapProcessor);

            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            if (level == null)
                return;

            foreach (LDtkComponentLayer layer in level.LayerInstances)
            {
                if (layer == null)
                    continue;

                switch (layer.Identifier)
                {
                    case LDtkIdentifiers.LogicMap:
                        layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Wall"));
                        layer.gameObject.SetTagRecursively("Wall");
                        break;

                    case LDtkIdentifiers.AutoTiles:
                        layer.gameObject.SetLayerRecursively(
                            LayerMask.NameToLayer("Static Object")
                        );
                        break;

                    case LDtkIdentifiers.ManualTiles:
                        layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Default"));
                        break;

                    case LDtkIdentifiers.Entities:
                        layer.gameObject.SetLayerRecursively(
                            LayerMask.NameToLayer("Dynamic Object")
                        );
                        break;
                }
            }
        }
    }
}
