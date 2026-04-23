using System.Collections.Generic;
using UnityEngine;

namespace GameMain.RunTime
{
    [RequireComponent(typeof(MovePlatformerLdtkLoadHandler))]
    public class MovePlatformer : ScalableEntity
    {
        public enum LoopType
        {
            Once,
            Loop,
            PingPong,
        }

        [Header("Settings")]
        [SerializeField]
        private float m_MoveSpeed = 2.0f;

        [SerializeField]
        private float m_WaitTime = 0.5f;

        [SerializeField]
        private LoopType m_LoopType = LoopType.Loop;

        [SerializeField]
        private bool m_AutoStart = true;

        [Header("Path")]
        [SerializeField]
        private List<Vector3> PathPoints = new();

        private int m_CurrentPointIndex = 0;
        private bool m_IsMoving = false;
        private bool m_IsWaiting = false;
        private float m_WaitTimer = 0.0f;
        private int m_Direction = 1;

        public void SetPathPoints(List<Vector3> points) => PathPoints = points;

        public void SetMoveSpeed(float speed) => m_MoveSpeed = speed;

        public void SetWaitTime(float time) => m_WaitTime = time;

        public void SetLoopType(LoopType type) => m_LoopType = type;

        public void SetAutoStart(bool autoStart) => m_AutoStart = autoStart;

        private void Start()
        {
            if (m_AutoStart && PathPoints != null && PathPoints.Count > 1)
            {
                StartMoving();
            }
        }

        public void StartMoving()
        {
            if (PathPoints == null || PathPoints.Count < 2)
            {
                return;
            }

            m_IsMoving = true;
            m_IsWaiting = false;
            m_CurrentPointIndex = 0;
            m_Direction = 1;
            transform.position = PathPoints[0];
        }

        private void Update()
        {
            if (!m_IsMoving || PathPoints == null || PathPoints.Count < 2)
                return;

            if (m_IsWaiting)
            {
                m_WaitTimer += Time.deltaTime;
                if (m_WaitTimer >= m_WaitTime)
                {
                    m_IsWaiting = false;
                    m_WaitTimer = 0.0f;
                    GetNextTargetIndex();
                }
                return;
            }

            Vector3 target = PathPoints[m_CurrentPointIndex];
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                m_MoveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, target) < 0.001f)
            {
                m_IsWaiting = true;
            }
        }

        private void GetNextTargetIndex()
        {
            switch (m_LoopType)
            {
                case LoopType.Once:
                    if (m_CurrentPointIndex < PathPoints.Count - 1)
                    {
                        m_CurrentPointIndex++;
                    }
                    else
                    {
                        m_IsMoving = false;
                    }
                    break;

                case LoopType.Loop:
                    m_CurrentPointIndex = (m_CurrentPointIndex + 1) % PathPoints.Count;
                    break;

                case LoopType.PingPong:
                    m_CurrentPointIndex += m_Direction;
                    if (m_CurrentPointIndex >= PathPoints.Count)
                    {
                        m_CurrentPointIndex = PathPoints.Count - 2;
                        m_Direction = -1;
                    }
                    else if (m_CurrentPointIndex < 0)
                    {
                        m_CurrentPointIndex = 1;
                        m_Direction = 1;
                    }

                    if (m_CurrentPointIndex < 0 || m_CurrentPointIndex >= PathPoints.Count)
                    {
                        m_IsMoving = false; // Safeguard
                    }
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            if (PathPoints == null || PathPoints.Count == 0)
                return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < PathPoints.Count; i++)
            {
                Gizmos.DrawSphere(PathPoints[i], 0.1f);
                if (i < PathPoints.Count - 1)
                {
                    Gizmos.DrawLine(PathPoints[i], PathPoints[i + 1]);
                }
                else if (m_LoopType == LoopType.Loop)
                {
                    Gizmos.DrawLine(PathPoints[i], PathPoints[0]);
                }
            }
        }
    }
}
