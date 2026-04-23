using System;
using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class SnakeHoney : MonoBehaviour
    {
        public bool IsCollected { get; private set; }

        private IDisposable m_DeathSubscription;

        private void Start()
        {
            m_DeathSubscription = MessageBroker.Global.Subscribe<SnakeGameEvents.SnakeDeathEvent>(
                _ => ResetHoney()
            );
        }

        private void OnDestroy()
        {
            m_DeathSubscription?.Dispose();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsCollected && other.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            IsCollected = true;
            gameObject.SetActive(false);
        }

        public void ResetHoney()
        {
            IsCollected = false;
            gameObject.SetActive(true);
        }
    }
}
