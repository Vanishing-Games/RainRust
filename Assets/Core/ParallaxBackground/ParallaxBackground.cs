using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum ParallaxClampMode
    {
        None,
        Repeat,
        Mirror,
    }

    [Serializable]
    public class ParallaxLayer
    {
        [Tooltip("背景层渲染的图片")]
        public Sprite sprite;

        [Tooltip("视差系数 (0: 随相机移动/无穷远, 1: 在世界空间固定/近处)"), Range(0, 1)]
        public float parallaxFactor;

        [Tooltip("边界处理模式 (仅水平方向)")]
        public ParallaxClampMode clampMode;

        [Tooltip("模糊模式,0=Gaussian,1=Kawase"), Range(0, 1)]
        public float blurMode;

        [Tooltip("高斯模糊强度"), Range(0.001f, 10f)]
        public float blurIntensity;

        [HideInInspector]
        public GameObject layerObject;

        [HideInInspector]
        public SpriteRenderer[] renderers;

        [HideInInspector]
        public float textureWidth;
    }

    public class ParallaxBackground : MonoBehaviour
    {
        private static readonly int BlurIntensityProperty = Shader.PropertyToID("_BlurIntensity");
        private static readonly int BlurModeProperty = Shader.PropertyToID("_BlurMode");
        private MaterialPropertyBlock m_PropertyBlock;
        private Material m_BlurMaterial;

        private void Start()
        {
            if (m_TargetCamera == null)
            {
                m_TargetCamera = Camera.main;
            }

            if (m_TargetCamera != null)
            {
                m_LastCameraPosition = m_TargetCamera.transform.position;
            }

            InitializeLayers();
        }

        private void LateUpdate()
        {
            if (m_TargetCamera == null)
            {
                return;
            }

            Vector3 cameraPosition = m_TargetCamera.transform.position;
            Vector3 cameraDelta = cameraPosition - m_LastCameraPosition;

            foreach (var layer in m_Layers)
            {
                if (layer.layerObject == null)
                {
                    continue;
                }

                Vector3 movement = new(
                    cameraDelta.x * (1 - layer.parallaxFactor),
                    cameraDelta.y,
                    0
                );

                layer.layerObject.transform.position += movement;

                HandleHorizontalWrapping(layer, cameraPosition);
            }

            m_LastCameraPosition = cameraPosition;
        }

        private void InitializeLayers()
        {
            if (m_BlurShader == null)
            {
                m_BlurShader = Shader.Find("Vanish/Sprite-Blur");
            }

            m_PropertyBlock ??= new MaterialPropertyBlock();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(transform.GetChild(i).gameObject);
                else
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < m_Layers.Count; i++)
            {
                var layer = m_Layers[i];
                if (layer.sprite == null)
                    continue;

                GameObject container = new($"Layer_{i}_{layer.sprite.name}");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                layer.layerObject = container;

                layer.textureWidth = layer.sprite.bounds.size.x;
                if (layer.textureWidth <= 0)
                    continue;

                int count = (layer.clampMode == ParallaxClampMode.None) ? 1 : 3;
                layer.renderers = new SpriteRenderer[count];

                for (int j = 0; j < count; j++)
                {
                    GameObject child = new($"Sprite_{j}");
                    child.transform.SetParent(container.transform);

                    float xPos =
                        (layer.clampMode == ParallaxClampMode.None)
                            ? 0
                            : (j - 1) * layer.textureWidth;
                    child.transform.localPosition = new Vector3(xPos, 0, 0);

                    SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
                    sr.sprite = layer.sprite;
                    sr.sortingOrder = m_Layers.Count - i;

                    layer.renderers[j] = sr;
                }
            }

            UpdateLayerProperties();
        }

        private void UpdateLayerProperties()
        {
            m_PropertyBlock ??= new MaterialPropertyBlock();

            if (m_BlurShader == null)
            {
                m_BlurShader = Shader.Find("Vanish/Sprite-Blur");
            }

            if (m_BlurShader != null && m_BlurMaterial == null)
            {
                m_BlurMaterial = new Material(m_BlurShader);
            }

            foreach (var layer in m_Layers)
            {
                if (layer.renderers == null)
                    continue;

                foreach (var sr in layer.renderers)
                {
                    if (sr == null)
                        continue;

                    if (layer.blurIntensity > 0 && m_BlurShader != null)
                    {
                        sr.sharedMaterial = m_BlurMaterial;
                        sr.GetPropertyBlock(m_PropertyBlock);
                        m_PropertyBlock.SetFloat(BlurIntensityProperty, layer.blurIntensity);
                        m_PropertyBlock.SetFloat(BlurModeProperty, layer.blurMode);
                        sr.SetPropertyBlock(m_PropertyBlock);
                    }
                    else
                    {
                        sr.sharedMaterial = null; // Use default sprite material
                    }
                }
            }
        }

        private void HandleHorizontalWrapping(ParallaxLayer layer, Vector3 cameraPosition)
        {
            if (layer.clampMode == ParallaxClampMode.None || layer.renderers == null)
                return;

            float localCameraX = cameraPosition.x - layer.layerObject.transform.position.x;
            float width = layer.textureWidth;

            foreach (var sr in layer.renderers)
            {
                if (sr == null)
                    continue;

                Vector3 pos = sr.transform.localPosition;
                float offset = localCameraX - pos.x;

                if (Mathf.Abs(offset) > width * 1.5f)
                {
                    float shiftCount = Mathf.Sign(offset) * 3f;
                    pos.x += shiftCount * width;
                    sr.transform.localPosition = pos;

                    if (layer.clampMode == ParallaxClampMode.Mirror)
                    {
                        int index = Mathf.RoundToInt(pos.x / width);
                        sr.flipX = Mathf.Abs(index) % 2 != 0;
                    }
                }
            }
        }

        [SerializeField]
        private Camera m_TargetCamera;

        [SerializeField]
        private List<ParallaxLayer> m_Layers = new();

        [SerializeField]
        private Shader m_BlurShader;

        private Vector3 m_LastCameraPosition;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Layers == null)
                return;

            if (m_Layers.Count > 1)
            {
                m_Layers.Sort((a, b) => b.parallaxFactor.CompareTo(a.parallaxFactor));
            }

            if (transform.childCount > 0)
            {
                UpdateLayerProperties();
            }
        }

        [ContextMenu("一键分配视差系数")]
        private void DistributeFactors()
        {
            if (m_Layers == null || m_Layers.Count == 0)
                return;

            UnityEditor.Undo.RecordObject(this, "Distribute Parallax Factors");

            if (m_Layers.Count == 1)
            {
                m_Layers[0].parallaxFactor = 0.5f;
            }
            else
            {
                for (int i = 0; i < m_Layers.Count; i++)
                {
                    m_Layers[i].parallaxFactor = 1.0f - ((float)i / (m_Layers.Count - 1));
                    m_Layers[i].blurIntensity = Math.Clamp(
                        (1.0f - m_Layers[i].parallaxFactor) * 10f,
                        0.001f,
                        10f
                    );
                    m_Layers[i].clampMode = ParallaxClampMode.Repeat;
                }
            }

            if (transform.childCount > 0)
            {
                UpdateLayerProperties();
            }
        }

        [ContextMenu("生成图层对象")]
        private void GenerateLayerObjects() => Start();
#endif
    }
}
