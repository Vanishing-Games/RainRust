using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelTransition : MonoBehaviour
    {
        internal Vector3 GetPlayerFeetSpawnPoint()
        {
            return transform.position;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                s_CurrentTransitions.Add(this);
                CheckTransition();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                s_CurrentTransitions.Remove(this);
            }
        }

        private void OnDisable()
        {
            s_CurrentTransitions.Remove(this);
        }

        private void CheckTransition()
        {
            if (
                Target != null
                && s_CurrentTransitions.Contains(Target)
                && this == LevelManager.Instance.GetCurrentTransition()
            )
            {
                LevelManager.Instance.SwitchLevel(this);
            }
        }

        private static readonly HashSet<LevelTransition> s_CurrentTransitions = new();

        [LabelText("目标位置")]
        [SerializeField]
        public LevelTransition Target;

        [LabelText("索引 (Index)")]
        public int? Index;
    }
}
