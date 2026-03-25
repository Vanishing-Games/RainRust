using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor.Tools
{
    public class RecursiveMaterialSetter : OdinEditorWindow
    {
        [MenuItem("Tools/RainRust/Recursive Material Setter")]
        private static void OpenWindow()
        {
            GetWindow<RecursiveMaterialSetter>("Material Setter").Show();
        }

        [Title("Settings")]
        [Required("请指定一个根物体"), SceneObjectsOnly]
        [LabelText("目标物体")]
        public GameObject targetRoot;

        [Required("请指定要设置的材质")]
        [InlineEditor]
        [LabelText("目标材质")]
        public Material targetMaterial;

        [Button(ButtonSizes.Large, Name = "执行材质替换")]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        private void ApplyMaterials()
        {
            if (targetRoot == null || targetMaterial == null)
            {
                EditorUtility.DisplayDialog("错误", "请先分配目标物体和材质！", "确定");
                return;
            }

            // 获取所有 Renderer（包括禁用的）
            Renderer[] renderers = targetRoot.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "提示",
                    "在该物体及其子物体中未找到任何 Renderer 组件。",
                    "确定"
                );
                return;
            }

            // 注册撤销操作
            Undo.RecordObjects(renderers, "Recursive Material Replacement");

            int count = 0;
            foreach (var renderer in renderers)
            {
                // 将该 Renderer 的所有材质槽位都设置为目标材质
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = targetMaterial;
                }
                renderer.sharedMaterials = materials;
                count++;
            }

            // 保存更改
            EditorUtility.SetDirty(targetRoot);

            Debug.Log(
                $"<color=green>[MaterialSetter]</color> 已成功将 <b>{count}</b> 个 Renderer 的材质设置为 <i>{targetMaterial.name}</i>"
            );
            EditorUtility.DisplayDialog("完成", $"已成功处理了 {count} 个 Renderer。", "确定");
        }
    }
}
