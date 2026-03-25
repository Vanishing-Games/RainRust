using Core;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public class LDtkMaterialSetProcessor : LDtkPostprocessor
    {
        private const string TargetMaterialPath =
            "Assets/Rendering/RainRust/Materials/material_rainRust_default.mat";

        public override int GetPostprocessOrder() => 10;

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            CLogger.LogInfo(
                $"[MaterialSetProcessor] Post process LDtk level: {root.name}",
                LogTag.LdtkLogicMapProcessor
            );

            Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(TargetMaterialPath);

            if (targetMaterial == null)
            {
                CLogger.LogError(
                    $"[MaterialSetProcessor] 未能在路径找到目标材质: {TargetMaterialPath}",
                    LogTag.LdtkLogicMapProcessor
                );
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            int count = 0;
            foreach (var renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = targetMaterial;
                }
                renderer.sharedMaterials = materials;
                count++;
            }

            CLogger.LogInfo(
                $"[MaterialSetProcessor] 已成功将 {root.name} 中 {count} 个 Renderer 的材质设置为 {targetMaterial.name}",
                LogTag.LDtkMaterialSetProcessor
            );
        }
    }
}
