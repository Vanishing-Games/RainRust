using System;
using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public enum HoneyState
    {
        Uncollected,
        CollectedTemporary,
        CollectedPermanent,
    }

    public class SnakeHoney : AutoLdtkEntity
    {
        private void Awake()
        {
            MessageBroker
                .Global.Receive<GamePlaySnakeGameEvents.SnakeDeathEvent>()
                .Subscribe(_ => OnSnakeDeath())
                .AddTo(ref m_Disposables);

            MessageBroker
                .Global.Receive<GamePlaySnakeGameEvents.SnakeSaveEvent>()
                .Subscribe(_ => OnSnakeSave())
                .AddTo(ref m_Disposables);
        }

        private void OnDestroy()
        {
            m_Disposables.Dispose();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (m_State != HoneyState.Uncollected)
                return;

            if (other.GetComponentInParent<PlayerSnake>() != null)
            {
                Collect();
            }
        }

        private void Collect()
        {
            m_State = HoneyState.CollectedTemporary;
            gameObject.SetActive(false);
            MessageBroker.Global.Publish(
                new GamePlaySnakeGameEvents.HoneyCollectedEvent { Honey = this }
            );
        }

        private void OnSnakeDeath()
        {
            if (m_State == HoneyState.CollectedTemporary)
            {
                m_State = HoneyState.Uncollected;
                gameObject.SetActive(true);
                MessageBroker.Global.Publish(
                    new GamePlaySnakeGameEvents.HoneyResetEvent { Honey = this }
                );
            }
        }

        private void OnSnakeSave()
        {
            if (m_State == HoneyState.CollectedTemporary)
            {
                m_State = HoneyState.CollectedPermanent;
            }
        }

        public HoneyState State => m_State;
        private HoneyState m_State = HoneyState.Uncollected;
        private DisposableBag m_Disposables = new();
    }
}
