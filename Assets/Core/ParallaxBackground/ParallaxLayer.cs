using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "NewParallaxLayer", menuName = "Parallax/Layer")]
    public class ParallaxLayer : VgSerializedScriptableObject
    {
        [Tooltip("背景层渲染的图片")]
        public Sprite sprite;

        [Tooltip("水平视差系数 (0: 随相机移动/无穷远, 1: 在世界空间固定/近处)"), Range(0, 1)]
        public float parallaxFactorX;

        [Tooltip("垂直视差系数 (0: 随相机移动/无穷远, 1: 在世界空间固定/近处)"), Range(0, 1)]
        public float parallaxFactorY;

        [Tooltip("边界处理模式 (仅水平方向)")]
        public ParallaxClampMode clampMode;

        [Tooltip("模糊模式,0=Gaussian,1=Kawase"), Range(0, 1)]
        public float blurMode;

        [Tooltip("高斯模糊强度"), Range(0.001f, 10f)]
        public float blurIntensity = 0.001f;

        [NonSerialized, HideInInspector]
        public GameObject layerObject;

        [NonSerialized, HideInInspector]
        public SpriteRenderer[] renderers;

        [NonSerialized, HideInInspector]
        public float textureWidth;
    }
}
