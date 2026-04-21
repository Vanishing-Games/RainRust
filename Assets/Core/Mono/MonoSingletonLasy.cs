using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Thread safe implementation of the Singleton pattern using a lazy initialization.
    /// </summary>
    /// <typeparam name="T">The type of the singleton.</typeparam>
    public class MonoSingletonLasy<T> : MonoBehaviour
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
                        }
                    }
                    return m_Instance;
                }
            }
        }

        private static T m_Instance;
        private static readonly object m_LockObject = new();
    }
}
