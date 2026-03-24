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
                        layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("LogicMap"));
                        layer.gameObject.SetTagRecursively("LogicMap");
                        break;

                    case LDtkIdentifiers.AutoTiles:
                        layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("AutoTile"));
                        layer.gameObject.SetTagRecursively("AutoTile");

                        break;

                    case LDtkIdentifiers.ManualTiles:
                        layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("ManualTile"));
                        layer.gameObject.SetTagRecursively("ManualTile");

                        break;

                    case LDtkIdentifiers.Entities:
                        layer.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Entity"));
                        layer.gameObject.SetTagRecursively("Entity");

                        break;
                }
            }
        }
    }
}
