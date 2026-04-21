using UnityEngine;

namespace Core
{
    /// <summary>
    /// Persistent while application is running.
    /// Thread safe implementation of Singleton pattern using lasy initialization.
    /// </summary>
    public class MonoSingletonPersistent<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                lock (m_LockObject)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = FindFirstObjectByType<T>();

                        if (m_Instance == null)
                        {
                            GameObject singletonObject = new(typeof(T).Name);
                            m_Instance = singletonObject.AddComponent<T>();
                            DontDestroyMe(m_Instance.transform);
                        }
                    }
                    return m_Instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (m_Instance == null)
            {
                m_Instance = this as T;
                DontDestroyMe(transform);
            }
            else if (m_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private static void DontDestroyMe(Transform me)
        {
            if (me.parent == null)
                DontDestroyOnLoad(me.gameObject);
            else
                DontDestroyMe(me.parent);
        }

        private static T m_Instance;
        private static readonly object m_LockObject = new();
    }
}
