using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;

namespace Demos.LDtkDemo.Editor
{
    public class RainRustLDtkPostProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessProject(GameObject root)
        {
            Debug.Log($"Post process LDtk project: {root.name}");
            LDtkComponentProject project = root.GetComponent<LDtkComponentProject>();
            foreach (LDtkComponentWorld world in project.Worlds)
            {
                foreach (LDtkComponentLevel level in world.Levels)
                {
                    foreach (LDtkComponentLayer layer in level.LayerInstances)
                    {
                        LDtkComponentLayerTilesetTiles tiles = layer.GridTiles;
                        //access tile data!

                        LDtkComponentLayerIntGridValues intGrid = layer.IntGrid;
                        //access intGrid data!

                        foreach (LDtkComponentEntity entity in layer.EntityInstances)
                        {
                            //access entities!
                        }
                    }
                }
            }
        }

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            Debug.Log($"Post process LDtk level: {root.name}");
            LDtkComponentLevel level = root.GetComponent<LDtkComponentLevel>();
            foreach (LDtkComponentLayer layer in level.LayerInstances)
            {
                //iterate upon layers
            }
        }
    }
}
