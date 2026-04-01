using System.Collections.Generic;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// A lightweight, robust debug information display system.
    /// Usage: DebugUIManager.Log("Speed", player.speed);
    /// </summary>
    [ExecuteInEditMode]
    public class DebugUIManager : MonoBehaviour
    {
        private void OnEnable()
        {
            if (m_Instance != null && m_Instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            m_Instance = this;

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            // Calculate smoothed delta time for accurate FPS reading
            if (m_ShowFPS)
            {
                m_DeltaTime += (Time.unscaledDeltaTime - m_DeltaTime) * 0.1f;
            }
        }

        /// <summary>
        /// Logs or updates a debug value.
        /// </summary>
        /// <param name="name">Unique name for this entry</param>
        /// <param name="value">The object to display (ToString() will be called)</param>
        public static void Log(string name, object value)
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<DebugUIManager>();
                if (m_Instance == null)
                    return;
            }

            string valStr = value != null ? value.ToString() : "null";

            if (!m_Instance.m_DebugEntries.ContainsKey(name))
            {
                m_Instance.m_OrderedKeys.Add(name);
            }
            m_Instance.m_DebugEntries[name] = valStr;
        }

        private void OnGUI()
        {
            // Do not draw anything if no logs exist and FPS is disabled (unless in Editor)
            if (m_OrderedKeys.Count == 0 && !m_ShowFPS && !Application.isEditor)
                return;

            UpdateStyles();

            // Calculate Area Rect
            float w = Screen.width - m_Padding.x * 2;
            float h = Screen.height - m_Padding.y * 2;
            Rect areaRect = new(m_Padding.x, m_Padding.y, w, h);

            GUILayout.BeginArea(areaRect);

            // Outer Box with dynamic height
            GUILayout.BeginVertical(
                m_BoxStyle,
                GUILayout.MinWidth(m_MinWidth),
                GUILayout.ExpandWidth(false)
            );

            // --- 1. Draw FPS if enabled ---
            if (m_ShowFPS)
            {
                float msec = m_DeltaTime * 1000.0f;
                float fps = 1.0f / m_DeltaTime;
                
                // Color coding based on FPS performance
                string colorHex = fps >= 60f ? "#00FF00" : (fps >= 30f ? "#FFFF00" : "#FF0000");
                string fpsText = $"<b>FPS:</b> <color={colorHex}>{fps:0.}</color> ({msec:0.0} ms)";
                
                GUILayout.Label(fpsText, m_LabelStyle);

                // Add spacing if we also have active logs
                if (m_OrderedKeys.Count > 0)
                {
                    GUILayout.Space(m_Spacing);
                }
            }

            // --- 2. Draw Custom Logs ---
            if (m_OrderedKeys.Count == 0 && !m_ShowFPS)
            {
                GUILayout.Label("<color=grey><i>No active logs...</i></color>", m_LabelStyle);
            }
            else
            {
                foreach (var key in m_OrderedKeys)
                {
                    GUILayout.Label($"<b>{key}:</b> {m_DebugEntries[key]}", m_LabelStyle);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void UpdateStyles()
        {
            if (m_BackgroundTexture == null)
            {
                m_BackgroundTexture = new Texture2D(1, 1);
            }

            // Only apply texture changes if needed to be more efficient
            Color currentColor = m_BackgroundTexture.GetPixel(0, 0);
            if (currentColor != m_BackgroundColor)
            {
                m_BackgroundTexture.SetPixel(0, 0, m_BackgroundColor);
                m_BackgroundTexture.Apply();
            }

            m_BoxStyle ??= new GUIStyle();
            m_BoxStyle.normal.background = m_BackgroundTexture;
            m_BoxStyle.padding = new RectOffset(8, 8, 8, 8);

            m_LabelStyle ??= new GUIStyle(GUI.skin.label) { richText = true, wordWrap = false };

            m_LabelStyle.fontSize = m_FontSize;
            m_LabelStyle.normal.textColor = m_TextColor;
            m_LabelStyle.fontStyle = m_FontStyle;
            m_LabelStyle.alignment = TextAnchor.MiddleLeft;
        }

        private void OnDestroy()
        {
            if (m_BackgroundTexture != null)
            {
                DestroyImmediate(m_BackgroundTexture);
            }
        }

        [Header("General Settings")]
        public bool m_ShowFPS = true;

        [Header("Style Settings")]
        public Color m_BackgroundColor = new(0, 0, 0, 0.7f);
        public Color m_TextColor = Color.white;
        public int m_FontSize = 14;
        public FontStyle m_FontStyle = FontStyle.Normal;

        [Header("Layout Settings")]
        public Vector2 m_Padding = new(10, 10);
        public float m_MinWidth = 350f;
        public float m_Spacing = 5f;

        private static DebugUIManager m_Instance;
        private Dictionary<string, string> m_DebugEntries = new();
        private List<string> m_OrderedKeys = new();

        private GUIStyle m_BoxStyle;
        private GUIStyle m_LabelStyle;
        private Texture2D m_BackgroundTexture;
        
        // Variables for FPS calculation
        private float m_DeltaTime = 0.0f;
    }
}