using System.Collections.Generic;
using GameMain.RunTime;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    [CustomEditor(typeof(DebugUIManager))]
    public class DebugUIManagerEditor : UnityEditor.Editor
    {
        private bool _showRawData = false;

        public override void OnInspectorGUI()
        {
            DebugUIManager manager = (DebugUIManager)target;

            // 1. Header and Quick Help
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Debug UI System", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Usage: DebugUIManager.Log(\"Key\", value);\nWorks in both Runtime and Editor.",
                MessageType.Info
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // 2. Main Settings
            serializedObject.Update();

            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontStyle"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("padding"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"));

            serializedObject.ApplyModifiedProperties();

            // 3. Live Data Monitor
            EditorGUILayout.Space(15);
            _showRawData = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showRawData,
                "Live Runtime Data"
            );

            if (_showRawData)
            {
                if (!Application.isPlaying && !Application.isEditor)
                {
                    EditorGUILayout.HelpBox(
                        "Data will appear here during playback.",
                        MessageType.None
                    );
                }
                else
                {
                    var entriesField = typeof(DebugUIManager).GetField(
                        "_debugEntries",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );
                    var orderedKeysField = typeof(DebugUIManager).GetField(
                        "_orderedKeys",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );

                    if (entriesField != null && orderedKeysField != null)
                    {
                        var entries = entriesField.GetValue(manager) as Dictionary<string, string>;
                        var orderedKeys = orderedKeysField.GetValue(manager) as List<string>;

                        if (orderedKeys != null && orderedKeys.Count > 0)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.textArea);
                            foreach (var key in orderedKeys)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(
                                    key,
                                    EditorStyles.miniBoldLabel,
                                    GUILayout.Width(100)
                                );
                                EditorGUILayout.SelectableLabel(
                                    entries[key],
                                    EditorStyles.miniLabel,
                                    GUILayout.Height(18)
                                );
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.EndVertical();

                            if (GUILayout.Button("Clear Logs", EditorStyles.miniButton))
                            {
                                entries.Clear();
                                orderedKeys.Clear();
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(
                                "No active logs.",
                                EditorStyles.centeredGreyMiniLabel
                            );
                        }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
