using LDtkUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelBoundsTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                CheckLevelSwitch(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                CheckLevelSwitch(other);
        }

        private void CheckLevelSwitch(Collider2D playerCollider)
        {
            var level = GetComponentInParent<LDtkComponentLevel>();
            var levelManager = LevelManager.Instance;
            if (level == null || level == levelManager.CurrentLevel)
                return;

            var room = GetComponentInParent<LevelRoom>();
            if (room != null && room.BorderBounds.Contains(playerCollider.bounds.center))
                levelManager.SwitchLevel(level);
        }
    }
}
