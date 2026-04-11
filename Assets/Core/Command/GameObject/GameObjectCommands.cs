using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class GameObjectCommands
    {
        public class InstantiateGoCommand : ICommand<GameObject>, IUndoableCommand<bool>
        {
            public InstantiateGoCommand(GameObject prefab)
                : this(prefab, Vector3.zero, Quaternion.identity, Vector3.one) { }

            public InstantiateGoCommand(GameObject prefab, Vector3 position)
                : this(prefab, position, Quaternion.identity, Vector3.one) { }

            public InstantiateGoCommand(GameObject prefab, Vector3 position, Quaternion rotation)
                : this(prefab, position, rotation, Vector3.one) { }

            public InstantiateGoCommand(
                GameObject prefab,
                Vector3 position,
                Quaternion rotation,
                Vector3 scale
            )
            {
                m_Prefab = prefab;
                m_Position = position;
                m_Rotation = rotation;
                m_Scale = scale;
            }

            public GameObject Execute()
            {
                if (m_Prefab == null)
                {
                    CLogger.LogError("InstantiateGoCommand: Prefab is null!", LogTag.Command);
                    return null;
                }

                m_Instance = Object.Instantiate(m_Prefab, m_Position, m_Rotation);
                if (m_Instance != null)
                {
                    m_Instance.transform.localScale = m_Scale;
                }
                return m_Instance;
            }

            public bool Undo()
            {
                if (m_Instance != null)
                {
                    Object.Destroy(m_Instance);
                    return true;
                }
                return false;
            }

            private readonly GameObject m_Prefab;
            private readonly Vector3 m_Position;
            private readonly Quaternion m_Rotation;
            private readonly Vector3 m_Scale;
            private GameObject m_Instance;
        }
    }
}
