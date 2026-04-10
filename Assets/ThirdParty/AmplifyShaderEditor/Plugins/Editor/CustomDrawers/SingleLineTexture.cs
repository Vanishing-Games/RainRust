// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEditor;
using UnityEngine;

namespace AmplifyShaderEditor
{
    public class SingleLineTexture : MaterialPropertyDrawer
    {
        public override void OnGUI(
            Rect position,
            MaterialProperty prop,
            String label,
            MaterialEditor editor
        )
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            Texture value = editor.TexturePropertyMiniThumbnail(
                position,
                prop,
                label,
                string.Empty
            );

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.textureValue = value;
            }
        }
    }
}
