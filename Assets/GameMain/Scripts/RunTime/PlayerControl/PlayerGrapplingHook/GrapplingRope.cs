using Core;
using Sirenix.OdinInspector;
using UnityEngine;
using VanishingGames.ECC.Runtime;

namespace GameMain.RunTime
{
    public class GrapplingRope : MonoBehaviour
    {
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            m_LineRenderer = GetComponent<LineRenderer>();
            m_Owner = GetComponentInParent<EccSystem>();

            if (m_Owner == null)
            {
                CLogger.LogError(
                    "GrapplingRope:Can't Find EccSystem, Owner is null",
                    LogTag.PlayerControl
                );
                this.enabled = false;
                return;
            }
            m_PlayerControlComponent = m_Owner.GetEccComponent<PlayerControlComponent>();

            m_Spring = new Spring();
            m_Spring.SetTarget(0);
        }

        //Called after Update to ensure the rope is drawn after the player has moved
        void LateUpdate()
        {
            DrawRope();
        }

        /// <summary>
        /// 什么时候开始绘制绳子
        /// </summary>
        /// <returns></returns>
        bool ShouldDrawRope()
        {
            return m_PlayerControlComponent.IsThrowing();
        }

        void DrawRope()
        {
            if (!ShouldDrawRope())
            {
                m_CurrentAnimGrapplePosition = m_PlayerControlComponent.GetGrappleTipPosition();
                m_Spring.Reset();
                if (m_LineRenderer.positionCount > 0)
                    m_LineRenderer.positionCount = 0;

#if UNITY_EDITOR
                Time.timeScale = 1f;
#endif
                return;
            }

            if (m_LineRenderer.positionCount == 0)
            {
                m_Spring.SetVelocity(m_Velocity);
                m_LineRenderer.positionCount = m_Quality + 1;
            }

            m_Spring.SetDamper(m_Damper);
            m_Spring.SetStrength(m_Strength);
            m_Spring.Update(Time.deltaTime);

            var grapplePoint = m_PlayerControlComponent.GetGrapplePosition();
            var roapTipPosition = m_PlayerControlComponent.GetGrappleTipPosition();
            var up =
                Quaternion.LookRotation((grapplePoint - roapTipPosition).normalized) * Vector3.up;

            m_CurrentAnimGrapplePosition = m_EnableAnimLerp
                ? Vector3.Lerp(
                    m_CurrentAnimGrapplePosition,
                    grapplePoint,
                    Time.deltaTime * m_RopeAnimSpeed
                )
                : grapplePoint;

            for (var i = 0; i < m_Quality + 1; i++)
            {
                var delta = i / (float)m_Quality;
                var offset =
                    m_AffectCurve.Evaluate(delta)
                    * m_Spring.Value
                    * m_WaveHeight
                    * Mathf.Sin(delta * m_WaveCount * Mathf.PI)
                    * up;

                m_LineRenderer.SetPosition(
                    i,
                    Vector3.Lerp(roapTipPosition, m_CurrentAnimGrapplePosition, delta) + offset
                );
            }

#if UNITY_EDITOR
            Time.timeScale = debug_TimeScale;

            debug_RopeStartPosition = roapTipPosition;
            debug_RopeEndPosition = grapplePoint;
            debug_CurrentAnimGrapplePosition = m_CurrentAnimGrapplePosition;
#endif
        }

        private EccSystem m_Owner;
        private PlayerControlComponent m_PlayerControlComponent;
        private LineRenderer m_LineRenderer;
        private Spring m_Spring;
        public Vector3 m_CurrentAnimGrapplePosition;
        public bool m_EnableAnimLerp = false;
        public int m_Quality;
        public float m_Damper;
        public float m_Strength;
        public float m_Velocity;
        public float m_RopeAnimSpeed;
        public float m_WaveCount;
        public float m_WaveHeight;
        public AnimationCurve m_AffectCurve;

#if UNITY_EDITOR
        [Header("调试")]
        [ShowInInspector, ReadOnly, LabelText("绳子起点位置")]
        public Vector3 debug_RopeStartPosition;

        [ShowInInspector, ReadOnly, LabelText("绳子终点位置")]
        public Vector3 debug_RopeEndPosition;

        [ShowInInspector, ReadOnly, LabelText("绳子动画终点位置")]
        public Vector3 debug_CurrentAnimGrapplePosition;

        [ShowInInspector, LabelText("绳子启动时的时间缩放"), Range(0.01f, 1f)]
        public float debug_TimeScale = 1f;
#endif
    }
}
