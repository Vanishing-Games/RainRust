using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameMain.RunTime
{
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
        public string m_SpritePath; // 需要易于编辑, 能让用户直接将文件拖到这里就能自动设置路径
        public TailType m_TailType;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class SnakeTail : MonoBehaviour
    {
        public SpriteRenderer m_SpriteRenderer;
    }

    public class PlayerSnake : MonoBehaviour
    {
        private enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right,
        }

        public void SetUp(Vector3 startPosition)
        {
            gameObject.transform.position = startPosition;
        }

        private void Update()
        {
            if (!MoveTimer())
                return;

            MoveStep(GetInputDirection());
        }

        private bool MoveTimer()
        {
            return false;
        }

        private MoveDirection GetInputDirection()
        {
            var horizontal = VgInput.GetAxis(InputAxis.LeftStickHorizontal);
            var vertical = VgInput.GetAxis(InputAxis.LeftStickVertical);

            throw new NotImplementedException();
        }

        private void MoveStep(MoveDirection direction)
        {
            if (Collide(direction))
            {
                Respawn();
            }
            else
            {
                MoveHead();
                AddTail();
            }
        }

        private void Respawn()
        {
            throw new NotImplementedException();
        }

        private bool Collide(MoveDirection direction)
        {
            return false;
        }

        private void AddTail()
        {
            throw new NotImplementedException();
        }

        private void MoveHead()
        {
            throw new NotImplementedException();
        }

        private void ReSpawn() { }

        public List<SnakeTailRecord> m_TailLibrary = new();
        public List<SnakeTail> m_TailObjects = new();
        private Vector3 m_StartPos;
    }
}
