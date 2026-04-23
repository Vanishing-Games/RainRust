using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace GameMain.RunTime
{
    [Serializable]
    public class SnakeTailRecord
    {
        public enum TailType
        {
            [LabelText("直线尾巴, 从下到上")]
            Line,

            [LabelText("直角尾巴, 从下到右")]
            Corner,
        }

        [Tooltip("尾巴的Sprite路径，相对于Resources文件夹")]
        public string m_SpritePath;
        public TailType m_TailType;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class SnakeTail : MonoBehaviour
    {
        public SpriteRenderer m_SpriteRenderer;
        public bool IsPermanent { get; private set; }
        public string SpritePath { get; private set; }

        private void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSprite(string spritePath)
        {
            SpritePath = spritePath;
            if (string.IsNullOrEmpty(spritePath)) return;
            
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                if (m_SpriteRenderer == null) m_SpriteRenderer = GetComponent<SpriteRenderer>();
                m_SpriteRenderer.sprite = sprite;
            }
        }

        public void SetPermanent()
        {
            IsPermanent = true;
            if (m_SpriteRenderer == null) m_SpriteRenderer = GetComponent<SpriteRenderer>();
            var color = m_SpriteRenderer.color;
            color.a = 0.5f;
            m_SpriteRenderer.color = color;
        }
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class PlayerSnake : MonoBehaviour
    {
        private enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right,
        }

        [Serializable]
        public struct SavedTailData
        {
            public Vector2 Position;
            public string SpritePath;
            public float Rotation;
        }

        [Header("Settings")]
        public float m_MoveInterval = 0.2f;
        public float m_GridSize = 1.0f;
        public GameObject m_TailPrefab;
        public LayerMask m_ObstacleLayer;

        [Header("State")]
        [ShowInInspector, ReadOnly]
        private MoveDirection m_CurrentDirection = MoveDirection.Right;
        private MoveDirection m_NextDirection = MoveDirection.Right;
        private float m_MoveTimer;

        [OdinSerialize]
        public List<SnakeTailRecord> m_TailLibrary = new();
        public List<SnakeTail> m_ActiveTails = new();
        public List<SnakeTail> m_PermanentTails = new();
        private Vector3 m_StartPos;

        private void Reset()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 0.8f);
            }

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10;
            }
        }

        public void SetUp(Vector3 startPosition)
        {
            m_StartPos = startPosition;
            transform.position = startPosition;
            m_CurrentDirection = MoveDirection.Right;
            m_NextDirection = MoveDirection.Right;
            
            ClearActiveTails();
            LoadPermanentTails();
        }

        private void Update()
        {
            UpdateInput();

            if (!MoveTimer())
                return;

            MoveStep(m_NextDirection);
        }

        private void UpdateInput()
        {
            var horizontal = VgInput.GetAxis(InputAxis.LeftStickHorizontal);
            var vertical = VgInput.GetAxis(InputAxis.LeftStickVertical);

            if (Mathf.Abs(horizontal) > 0.5f)
            {
                if (horizontal > 0 && m_CurrentDirection != MoveDirection.Left)
                    m_NextDirection = MoveDirection.Right;
                else if (horizontal < 0 && m_CurrentDirection != MoveDirection.Right)
                    m_NextDirection = MoveDirection.Left;
            }
            else if (Mathf.Abs(vertical) > 0.5f)
            {
                if (vertical > 0 && m_CurrentDirection != MoveDirection.Down)
                    m_NextDirection = MoveDirection.Up;
                else if (vertical < 0 && m_CurrentDirection != MoveDirection.Up)
                    m_NextDirection = MoveDirection.Down;
            }
        }

        private bool MoveTimer()
        {
            m_MoveTimer += Time.deltaTime;
            if (m_MoveTimer >= m_MoveInterval)
            {
                m_MoveTimer -= m_MoveInterval;
                return true;
            }
            return false;
        }

        private void MoveStep(MoveDirection direction)
        {
            var previousDirection = m_CurrentDirection;
            m_CurrentDirection = direction;
            Vector3 nextPos = GetNextPosition(direction);

            if (Collide(nextPos))
            {
                Respawn();
            }
            else
            {
                AddTail(transform.position, previousDirection, direction);
                MoveHead(nextPos);
            }
        }

        private Vector3 GetNextPosition(MoveDirection direction)
        {
            Vector3 offset = direction switch
            {
                MoveDirection.Up => Vector3.up,
                MoveDirection.Down => Vector3.down,
                MoveDirection.Left => Vector3.left,
                MoveDirection.Right => Vector3.right,
                _ => Vector3.zero
            };
            return transform.position + offset * m_GridSize;
        }

        private void Respawn()
        {
            CLogger.LogInfo("Snake Died!", LogTag.Game);
            MessageBroker.Global.Publish(new SnakeGameEvents.SnakeDeathEvent());
            
            ClearActiveTails();
            transform.position = m_StartPos;
            m_CurrentDirection = MoveDirection.Right;
            m_NextDirection = MoveDirection.Right;
            m_MoveTimer = 0;
        }

        private bool Collide(Vector3 position)
        {
            var hit = Physics2D.OverlapPoint(position, m_ObstacleLayer);
            if (hit != null)
            {
                if (hit.GetComponent<SavePoint>() != null)
                {
                    return false;
                }

                if (hit.TryGetComponent<SnakeTail>(out var tail))
                {
                    return !tail.IsPermanent;
                }

                return true;
            }
            return false;
        }

        private void AddTail(Vector3 position, MoveDirection dir1, MoveDirection dir2)
        {
            if (m_TailPrefab == null) return;
            
            GameObject tailGo = Instantiate(m_TailPrefab, position, Quaternion.identity);
            if (tailGo.TryGetComponent<SnakeTail>(out var tail))
            {
                SnakeTailRecord.TailType targetType = (dir1 == dir2) ? SnakeTailRecord.TailType.Line : SnakeTailRecord.TailType.Corner;

                var validRecords = m_TailLibrary.Where(r => r.m_TailType == targetType).ToList();
                if (validRecords.Count > 0)
                {
                    var record = validRecords[UnityEngine.Random.Range(0, validRecords.Count)];
                    tail.SetSprite(record.m_SpritePath);
                }

                float rotation = 0f;
                if (targetType == SnakeTailRecord.TailType.Line)
                {
                    rotation = dir1 switch
                    {
                        MoveDirection.Up => 0f,
                        MoveDirection.Left => 90f,
                        MoveDirection.Down => 180f,
                        MoveDirection.Right => -90f,
                        _ => 0f
                    };
                }
                else
                {
                    if ((dir1 == MoveDirection.Up && dir2 == MoveDirection.Right) ||
                        (dir1 == MoveDirection.Left && dir2 == MoveDirection.Down))
                    {
                        rotation = 0f;
                    }
                    else if ((dir1 == MoveDirection.Left && dir2 == MoveDirection.Up) ||
                             (dir1 == MoveDirection.Down && dir2 == MoveDirection.Right))
                    {
                        rotation = 90f;
                    }
                    else if ((dir1 == MoveDirection.Down && dir2 == MoveDirection.Left) ||
                             (dir1 == MoveDirection.Right && dir2 == MoveDirection.Up))
                    {
                        rotation = 180f;
                    }
                    else if ((dir1 == MoveDirection.Right && dir2 == MoveDirection.Down) ||
                             (dir1 == MoveDirection.Up && dir2 == MoveDirection.Left))
                    {
                        rotation = -90f;
                    }
                }

                tail.transform.rotation = Quaternion.Euler(0, 0, rotation);
                m_ActiveTails.Add(tail);
            }
        }

        private void MoveHead(Vector3 nextPos)
        {
            transform.position = nextPos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("SavePoint") || other.GetComponent<SavePoint>() != null)
            {
                OnReachedSavePoint(other.transform.position);
            }
        }

        private void OnReachedSavePoint(Vector3 position)
        {
            CLogger.LogInfo("Snake reached SavePoint", LogTag.Game);
            
            foreach (var tail in m_ActiveTails)
            {
                tail.SetPermanent();
                m_PermanentTails.Add(tail);
            }
            m_ActiveTails.Clear();
            
            m_StartPos = position;
            SavePermanentTails();
            
            MessageBroker.Global.Publish(new SnakeGameEvents.SnakeReachedSavePointEvent());
        }

        private void ClearActiveTails()
        {
            foreach (var tail in m_ActiveTails)
            {
                if (tail != null) Destroy(tail.gameObject);
            }
            m_ActiveTails.Clear();
        }

        private void SavePermanentTails()
        {
            var data = m_PermanentTails.Select(t => new SavedTailData
            {
                Position = t.transform.position,
                SpritePath = t.SpritePath,
                Rotation = t.transform.eulerAngles.z
            }).ToList();
            VgSaveSystem.Instance.UpdateSaveValue(GetSaveKey(), data);
        }

        private void LoadPermanentTails()
        {
            foreach (var tail in m_PermanentTails)
            {
                if (tail != null) Destroy(tail.gameObject);
            }
            m_PermanentTails.Clear();

            var data = VgSaveSystem.Instance.GetSaveValue<List<SavedTailData>>(GetSaveKey(), null);
            if (data != null && m_TailPrefab != null)
            {
                foreach (var item in data)
                {
                    GameObject tailGo = Instantiate(m_TailPrefab, (Vector3)item.Position, Quaternion.Euler(0, 0, item.Rotation));
                    if (tailGo.TryGetComponent<SnakeTail>(out var tail))
                    {
                        tail.SetSprite(item.SpritePath);
                        tail.SetPermanent();
                        m_PermanentTails.Add(tail);
                    }
                }
            }
        }

        private string GetSaveKey()
        {
            var level = GetComponentInParent<LDtkUnity.LDtkComponentLevel>();
            string levelId = level != null ? level.Identifier : "DefaultLevel";
            return $"PlayerSnake_PermanentTails_{levelId}";
        }
    }
}
