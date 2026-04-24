using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GameMain.RunTime
{
    public enum TailType
    {
        [LabelText("直线尾巴, 从下到上")]
        Line,

        [LabelText("直角尾巴, 从下到右")]
        Corner,
    }

    public class PlayerSnake : MonoBehaviour
    {
        private void OnEnable()
        {
            MessageBroker
                .Global.Subscribe<GamePlaySnakeGameEvents.SnakeSaveEvent>(_ => OnSave())
                .AddTo(ref m_Disposables);
        }

        private void OnDisable()
        {
            m_Disposables.Dispose();
        }

        private void Start()
        {
            LoadTailLibrary();
            EnsureTailsContainer();
            if (m_StartPos == Vector3.zero)
                m_StartPos = transform.position;
        }

        private void Update()
        {
            HandleInput();

            m_Timer += Time.deltaTime;
            if (m_Timer >= m_MoveInterval)
            {
                m_Timer -= m_MoveInterval;
                Move();
            }
        }

        public void SetUp(Vector3 spawnPosition)
        {
            transform.position = spawnPosition + new Vector3(0.5f, 0.5f, 0);
            m_StartPos = transform.position;
        }

        private void EnsureTailsContainer()
        {
            if (m_TailContainer == null)
            {
                var go = new GameObject("Snake_Tails_Container");
                m_TailContainer = go.transform;
            }
        }

        private void LoadTailLibrary()
        {
            m_TailLibrary[TailType.Line] = Resources.LoadAll<GameObject>(m_LineTailPath).ToList();
            m_TailLibrary[TailType.Corner] = Resources
                .LoadAll<GameObject>(m_CornerTailPath)
                .ToList();

            if (m_TailLibrary[TailType.Line].IsNullOrEmpty())
                CLogger.LogError(
                    $"[Snake] Cannot find LineTail prefab at Resources/{m_LineTailPath}",
                    LogTag.Game
                );

            if (m_TailLibrary[TailType.Corner].IsNullOrEmpty())
                CLogger.LogError(
                    $"[Snake] Cannot find CornerTail prefab at Resources/{m_CornerTailPath}",
                    LogTag.Game
                );
        }

        // ================= 输入 =================

        private Vector2Int? m_PendingDirection;

        private Vector2? HandleInput()
        {
            Vector2 input = VgInput.GetMovementVector();
            if (input.sqrMagnitude < 0.1f)
                return null;

            Vector2 dir;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                dir = input.x > 0 ? Vector2.right : Vector2.left;
            else
                dir = input.y > 0 ? Vector2.up : Vector2.down;

            m_PendingDirection = Vector2Int.RoundToInt(dir);
            return dir;
        }

        private void UpdateDirection()
        {
            if (m_PendingDirection == null)
                return;

            var next = m_PendingDirection.Value;

            // 防止反向
            if (next + m_CurrentDirection == Vector2Int.zero)
                return;

            m_CurrentDirection = next;
            m_PendingDirection = null;
        }

        // ================= 移动 =================

        private void Move()
        {
            UpdateDirection();

            Vector2Int dir = m_CurrentDirection;

            if (IsBlocked(dir))
            {
                Die();
                return;
            }

            Vector3 tailPos = m_Head != null ? m_Head.position : transform.position;
            SpawnTail(tailPos, dir);

            transform.position += new Vector3(dir.x, dir.y, 0);
        }

        private bool IsBlocked(Vector2Int direction)
        {
            Vector3 origin = m_Head != null ? m_Head.position : transform.position;
            Vector2 dir = new(direction.x, direction.y);

            float startOffset = 0f;
            Vector3 rayStart = origin + (Vector3)dir * startOffset;
            float distance = 0.5f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, dir, distance, m_ObstacleLayer);

            foreach (var hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (hit.collider.transform.IsChildOf(transform))
                    continue;

#if UNITY_EDITOR
                CLogger.LogInfo(
                    $"<color=red>[Snake Blocked]</color> Hit: {hit.collider.name} Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} at {hit.point}",
                    LogTag.Game
                );
#endif

                return true;
            }

            return false;
        }

        // ================= 尾巴 =================

        private Vector2Int m_LastDirection = Vector2Int.up;

        private TailType GetTailType()
        {
            if (m_LastDirection == m_CurrentDirection)
                return TailType.Line;

            return TailType.Corner;
        }

        private void SpawnTail(Vector3 position, Vector2 towards)
        {
            TailType type = GetTailType();

            if (!m_TailLibrary.TryGetValue(type, out var list) || list.IsNullOrEmpty())
                return;

            GameObject prefab = list[0]; // 可改为随机

            GameObject tailObj = Instantiate(
                prefab,
                position,
                Quaternion.identity,
                m_TailContainer
            );

            SnakeTail tail = tailObj.GetComponent<SnakeTail>();

            tail.Initialize(Vector2.one);
            tail.gameObject.layer = gameObject.layer;

            RotateTail(tail, towards);

            m_ActiveTails.Add(tail);

            m_LastDirection = m_CurrentDirection;
        }

        private void RotateTail(SnakeTail tail, Vector2 dir)
        {
            float angle = 0f;

            if (dir == Vector2.up)
                angle = 0;
            else if (dir == Vector2.right)
                angle = -90;
            else if (dir == Vector2.down)
                angle = 180;
            else if (dir == Vector2.left)
                angle = 90;

            tail.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // ================= 生命周期 =================

        private void Die()
        {
            CLogger.LogInfo("Snake Died!", LogTag.Game);
            MessageBroker.Global.Publish(new GamePlaySnakeGameEvents.SnakeDeathEvent());

            transform.position = m_StartPos;

            ResetSnake();
        }

        private void OnSave()
        {
            CLogger.LogInfo("Snake Saved!", LogTag.Game);

            foreach (var tail in m_ActiveTails)
                tail.SetPermanent();

            m_ActiveTails.Clear();
        }

        private void ResetSnake()
        {
            foreach (var tail in m_ActiveTails)
            {
                if (tail != null)
                {
                    tail.gameObject.SetActive(false);
                    Destroy(tail.gameObject);
                }
            }

            m_ActiveTails.Clear();
            m_Timer = 0;
            m_PendingDirection = null;
            m_CurrentDirection = Vector2Int.up;
            m_LastDirection = Vector2Int.up;
        }

        // ================= 配置 =================

        [SerializeField]
        private float m_MoveInterval = 0.5f;

        [SerializeField]
        private string m_LineTailPath = "SnakeTails/LineTail";

        [SerializeField]
        private string m_CornerTailPath = "SnakeTails/CornerTail";

        [SerializeField]
        private LayerMask m_ObstacleLayer;

        [SerializeField]
        private Transform m_Head;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool m_ShowDebug = true;

        private void OnDrawGizmos()
        {
            if (!m_ShowDebug)
                return;

            Vector3 origin = m_Head != null ? m_Head.position : transform.position;

            // 绘制当前移动方向 (绿色)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                origin,
                origin + new Vector3(m_CurrentDirection.x, m_CurrentDirection.y, 0)
            );

            // 绘制预输入方向 (黄色)
            if (m_PendingDirection.HasValue)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    origin,
                    origin + new Vector3(m_PendingDirection.Value.x, m_PendingDirection.Value.y, 0)
                );
            }

            // 绘制下一次移动的障碍物检测射线 (红色)
            Gizmos.color = Color.red;
            float distance = 0.8f;
            Vector3 dir = new(m_CurrentDirection.x, m_CurrentDirection.y, 0);
            Gizmos.DrawRay(origin, dir * distance);

            // 绘制射线末端的端点
            Gizmos.DrawWireSphere(origin + dir * distance, 0.05f);
        }
#endif

        // ================= 状态 =================

        private Vector2Int m_CurrentDirection = Vector2Int.up;
        private float m_Timer;
        private Vector3 m_StartPos;

        private List<SnakeTail> m_ActiveTails = new();
        private Dictionary<TailType, List<GameObject>> m_TailLibrary = new();

        private DisposableBag m_Disposables = new();
        private Transform m_TailContainer;
    }
}
