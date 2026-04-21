using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Core
{
    [InlineProperty]
    public struct FramedFloat
    {
        [HorizontalGroup("Row"), LabelText("Frames"), OdinSerialize]
        public int Frames
        {
            get => m_Frames;
            set
            {
                m_Frames = Mathf.Max(0, value);
                m_Seconds = m_Frames / BASE_FPS;
            }
        }

        [HorizontalGroup("Row"), LabelText("Seconds"), OdinSerialize]
        public float Seconds
        {
            get => m_Seconds;
            set
            {
                m_Seconds = Mathf.Max(0, value);
                m_Frames = Mathf.RoundToInt(m_Seconds * BASE_FPS);
            }
        }

        public static implicit operator float(FramedFloat f)
        {
            return f.m_Seconds;
        }

        public static implicit operator FramedFloat(float value)
        {
            return new FramedFloat { Seconds = value };
        }

        public const float BASE_FPS = 60f;
        private float m_Seconds;
        private int m_Frames;
    }
}
