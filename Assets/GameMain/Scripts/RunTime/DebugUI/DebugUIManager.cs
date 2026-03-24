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
        private static DebugUIManager _instance;
        private Dictionary<string, string> _debugEntries = new Dictionary<string, string>();
        private List<string> _orderedKeys = new List<string>();

        [Header("Style Settings")]
        public Color backgroundColor = new Color(0, 0, 0, 0.7f);
        public Color textColor = Color.white;
        public int fontSize = 14;
        public FontStyle fontStyle = FontStyle.Normal;

        [Header("Layout Settings")]
        public Vector2 padding = new Vector2(10, 10);
        public float minWidth = 150f;
        public float spacing = 5f;

        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;

        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            _instance = this;

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Logs or updates a debug value.
        /// </summary>
        /// <param name="name">Unique name for this entry</param>
        /// <param name="value">The object to display (ToString() will be called)</param>
        public static void Log(string name, object value)
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DebugUIManager>();
                if (_instance == null)
                    return;
            }

            string valStr = value != null ? value.ToString() : "null";

            if (!_instance._debugEntries.ContainsKey(name))
            {
                _instance._orderedKeys.Add(name);
            }
            _instance._debugEntries[name] = valStr;
        }

        private void OnGUI()
        {
            if (_orderedKeys.Count == 0 && !Application.isEditor)
                return;

            UpdateStyles();

            // Calculate Area Rect
            float w = Screen.width - padding.x * 2;
            float h = Screen.height - padding.y * 2;
            Rect areaRect = new Rect(padding.x, padding.y, w, h);

            GUILayout.BeginArea(areaRect);

            // Outer Box with dynamic height
            GUILayout.BeginVertical(
                _boxStyle,
                GUILayout.MinWidth(minWidth),
                GUILayout.ExpandWidth(false)
            );

            if (_orderedKeys.Count == 0)
            {
                GUILayout.Label("<color=grey><i>No active logs...</i></color>", _labelStyle);
            }
            else
            {
                foreach (var key in _orderedKeys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(
                        $"<b>{key}:</b>",
                        _labelStyle,
                        GUILayout.Width(minWidth * 0.4f)
                    );
                    GUILayout.Space(spacing);
                    GUILayout.Label(_debugEntries[key], _labelStyle);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void UpdateStyles()
        {
            if (_backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(1, 1);
            }

            _backgroundTexture.SetPixel(0, 0, backgroundColor);
            _backgroundTexture.Apply();

            if (_boxStyle == null)
                _boxStyle = new GUIStyle();
            _boxStyle.normal.background = _backgroundTexture;
            _boxStyle.padding = new RectOffset(8, 8, 8, 8);

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle();
                _labelStyle.richText = true;
                _labelStyle.wordWrap = false;
            }

            _labelStyle.fontSize = fontSize;
            _labelStyle.normal.textColor = textColor;
            _labelStyle.fontStyle = fontStyle;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
            {
                DestroyImmediate(_backgroundTexture);
            }
        }
    }
}
