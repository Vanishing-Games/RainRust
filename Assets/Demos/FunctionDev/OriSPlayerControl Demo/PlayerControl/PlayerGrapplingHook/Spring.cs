using UnityEngine;

namespace PlayerControlByOris
{
    public class Spring
    {
        public void Update(float deltaTime)
        {
            var direction = m_Target - m_Value >= 0 ? 1f : -1f;
            var force = Mathf.Abs(m_Target - m_Value) * m_Strength;
            m_Velocity += (force * direction - m_Velocity * m_Damper) * deltaTime;
            m_Value += m_Velocity * deltaTime;
        }

        public void Reset()
        {
            m_Velocity = 0f;
            m_Value = 0f;
        }

        public void SetValue(float value)
        {
            m_Value = value;
        }

        public void SetTarget(float target)
        {
            m_Target = target;
        }

        public void SetDamper(float damper)
        {
            m_Damper = damper;
        }

        public void SetStrength(float strength)
        {
            m_Strength = strength;
        }

        public void SetVelocity(float velocity)
        {
            m_Velocity = velocity;
        }

        public float Value => m_Value;

        private float m_Strength;
        private float m_Damper;
        private float m_Target;
        private float m_Velocity;
        private float m_Value;
    }
}
