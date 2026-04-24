using UnityEngine;

namespace GameMain.RunTime
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ChainJointTrigger : MonoBehaviour
    {
        private EntityChain m_ParentChain;
        private Rigidbody2D m_Rigidbody;

        public void Initialize(EntityChain parent)
        {
            m_ParentChain = parent;
            m_Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (m_ParentChain != null)
            {
                m_ParentChain.HandleJointTriggerEnter(other, m_Rigidbody);
            }
        }
    }
}
