namespace Core
{
    public class Singleton<T>
        where T : new()
    {
        public static T Instance
        {
            get
            {
                lock (m_LockObject)
                {
                    m_Instance ??= new T();
                    return m_Instance;
                }
            }
        }

        private static readonly object m_LockObject = new();
        private static T m_Instance;
    }
}
