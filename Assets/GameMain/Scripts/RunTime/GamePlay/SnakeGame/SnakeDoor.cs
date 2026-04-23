using System;
using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class SnakeDoor : MonoBehaviour
    {
        public string m_DoorId;
        public SnakeHoney[] DoorHoneys;

        private bool m_IsPermanentlyOpen;
        private bool m_IsCurrentlyOpen;
        private IDisposable m_SaveSubscription;

        private void Start()
        {
            // Load state from SaveSystem
            if (!string.IsNullOrEmpty(m_DoorId))
            {
                m_IsPermanentlyOpen = VgSaveSystem.Instance.GetSaveValue(GetSaveKey(), false);
            }

            if (m_IsPermanentlyOpen)
            {
                OpenPermanently();
            }
            else
            {
                m_SaveSubscription = MessageBroker.Global.Subscribe<SnakeGameEvents.SnakeReachedSavePointEvent>(
                    _ => OnReachedSavePoint()
                );
            }
        }

        private void OnDestroy()
        {
            m_SaveSubscription?.Dispose();
        }

        private void Update()
        {
            if (m_IsPermanentlyOpen)
                return;

            CheckHoneys();
        }

        private void CheckHoneys()
        {
            if (DoorHoneys == null || DoorHoneys.Length == 0)
                return;

            bool allCollected = true;
            foreach (var honey in DoorHoneys)
            {
                if (honey == null) continue;
                if (!honey.IsCollected)
                {
                    allCollected = false;
                    break;
                }
            }

            if (allCollected != m_IsCurrentlyOpen)
            {
                m_IsCurrentlyOpen = allCollected;
                UpdateDoorVisual();
            }
        }

        private void UpdateDoorVisual()
        {
            // Simple visual: hide/show gameObject or collider
            // In a real game we might play an animation
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.enabled = !m_IsCurrentlyOpen;
            }
            
            // Set Alpha or disable renderer for now
            if (TryGetComponent<SpriteRenderer>(out var renderer))
            {
                var color = renderer.color;
                color.a = m_IsCurrentlyOpen ? 0.3f : 1.0f;
                renderer.color = color;
            }
        }

        private void OnReachedSavePoint()
        {
            if (m_IsCurrentlyOpen && !m_IsPermanentlyOpen)
            {
                m_IsPermanentlyOpen = true;
                if (!string.IsNullOrEmpty(m_DoorId))
                {
                    VgSaveSystem.Instance.UpdateSaveValue(GetSaveKey(), true);
                }
                OpenPermanently();
            }
        }

        private void OpenPermanently()
        {
            m_IsCurrentlyOpen = true;
            UpdateDoorVisual();
            
            // Disable honeys as they are no longer needed
            if (DoorHoneys != null)
            {
                foreach (var honey in DoorHoneys)
                {
                    if (honey != null) honey.gameObject.SetActive(false);
                }
            }
            
            m_SaveSubscription?.Dispose();
            m_SaveSubscription = null;
        }

        private string GetSaveKey() => $"SnakeDoor_Open_{m_DoorId}";
    }
}
