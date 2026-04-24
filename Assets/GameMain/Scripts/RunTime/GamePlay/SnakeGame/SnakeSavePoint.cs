using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 贪吃蛇存档点：使用新管线
    /// </summary>
    public class SnakeSavePoint : LDtkTriggerEntity
    {
        private bool m_IsSaved;

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (m_IsSaved)
                return;

            // 检查是否是蛇
            if (other.GetComponentInParent<PlayerSnake>() != null)
            {
                m_IsSaved = true;
                OnSaveTriggered();
            }
        }

        private void OnSaveTriggered()
        {
            MessageBroker.Global.Publish(new GamePlaySnakeGameEvents.SnakeSaveEvent());
        }
    }
}
