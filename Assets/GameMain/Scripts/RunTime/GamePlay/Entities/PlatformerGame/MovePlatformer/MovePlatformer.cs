using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 移动平台：演示 SolidEntity 和复杂字段映射
    /// </summary>
    public class MovePlatformer : LDtkSolidEntity
    {
        public enum LoopType
        {
            Once,
            Loop,
            PingPong,
        }

        [Header("Settings")]
        [LDtkField]
        [SerializeField]
        private float MoveSpeed = 2.0f;

        [LDtkField]
        [SerializeField]
        private float WaitTime = 0.5f;

        [LDtkField("LoopType")]
        [SerializeField]
        private LoopType m_LoopType = LoopType.Loop;

        [LDtkField]
        [SerializeField]
        private bool AutoStart = true;

        [Header("Path")]
        [LDtkField]
        [SerializeField]
        private List<MovePlatformerPathPoint> PathPoints = new();

        private List<Vector3> m_WorldPathPoints = new();
        private int m_CurrentPointIndex = 0;
        private bool m_IsMoving = false;
        private bool m_IsWaiting = false;
        private float m_WaitTimer = 0.0f;
        private int m_Direction = 1;

        public override void OnPostImport()
        {
            m_WorldPathPoints = PathPoints
                .Select(p => new Vector3(p.transform.position.x, p.transform.position.y, 0))
                .ToList();
        }

        private void Start()
        {
            if (AutoStart && m_WorldPathPoints.Count > 1)
            {
                StartMoving();
            }
        }

        public void StartMoving()
        {
            if (m_WorldPathPoints.Count < 2)
                return;

            m_IsMoving = true;
            m_IsWaiting = false;
            m_CurrentPointIndex = 0;
            m_Direction = 1;
            transform.position = m_WorldPathPoints[0];
        }

        private void Update()
        {
            if (!m_IsMoving || m_WorldPathPoints.Count < 2)
                return;

            if (m_IsWaiting)
            {
                m_WaitTimer += Time.deltaTime;
                if (m_WaitTimer >= WaitTime)
                {
                    m_IsWaiting = false;
                    m_WaitTimer = 0.0f;
                    GetNextTargetIndex();
                }
                return;
            }

            Vector3 target = m_WorldPathPoints[m_CurrentPointIndex];
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                MoveSpeed * Time.deltaTime
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
                    if (m_CurrentPointIndex < m_WorldPathPoints.Count - 1)
                        m_CurrentPointIndex++;
                    else
                        m_IsMoving = false;
                    break;

                case LoopType.Loop:
                    m_CurrentPointIndex = (m_CurrentPointIndex + 1) % m_WorldPathPoints.Count;
                    break;

                case LoopType.PingPong:
                    m_CurrentPointIndex += m_Direction;
                    if (m_CurrentPointIndex >= m_WorldPathPoints.Count)
                    {
                        m_CurrentPointIndex = m_WorldPathPoints.Count - 2;
                        m_Direction = -1;
                    }
                    else if (m_CurrentPointIndex < 0)
                    {
                        m_CurrentPointIndex = 1;
                        m_Direction = 1;
                    }
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            var points = Application.isPlaying
                ? m_WorldPathPoints
                : PathPoints
                    .Select(p => new Vector3(p.transform.position.x, p.transform.position.y, 0))
                    .ToList();
            if (points == null || points.Count == 0)
                return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.DrawSphere(points[i], 0.1f);
                if (i < points.Count - 1)
                    Gizmos.DrawLine(points[i], points[i + 1]);
                else if (m_LoopType == LoopType.Loop)
                    Gizmos.DrawLine(points[i], points[0]);
            }
        }
    }
}
