using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using LDtkUnity;

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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CheckTransition();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                s_CurrentTransitions.Remove(this);
                
                var levelManager = LevelManager.Instance;
                if (this == levelManager.CurrentTransition)
                {
                    levelManager.ClearCurrentTransition();
                }
            }
        }

        private void OnDisable()
        {
            s_CurrentTransitions.Remove(this);
        }

        private void CheckTransition()
        {
            var levelManager = LevelManager.Instance;
            var myLevel = GetComponentInParent<LDtkComponentLevel>();
            
            if (myLevel == null || myLevel == levelManager.CurrentLevel) 
                return;

            // Ping-pong 保护：如果我们刚刚从某个入口进来，在离开那个入口的 Trigger 之前，
            // 不允许通过那个入口的 Target（也就是我们刚才出来的那个出口）切回去。
            if (levelManager.CurrentTransition != null && this == levelManager.CurrentTransition.Target)
            {
                return;
            }

            levelManager.SwitchLevel(this);
        }

        public static readonly HashSet<LevelTransition> s_CurrentTransitions = new();

        [LabelText("目标位置")]
        [SerializeField]
        public LevelTransition Target;

        [LabelText("索引 (Index)")]
        public int? Index;
    }
}
