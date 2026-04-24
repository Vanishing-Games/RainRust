using System.Collections.Generic;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class RuntimeEntityRegistry
    {
        private static readonly Dictionary<string, GameObject> m_IidToEntity = new();

        public static void Register(string iid, GameObject entity)
        {
            m_IidToEntity[iid] = entity;
        }

        public static GameObject GetEntity(string iid)
        {
            if (string.IsNullOrEmpty(iid))
                return null;
            m_IidToEntity.TryGetValue(iid, out var entity);
            return entity;
        }

        public static void Clear()
        {
            m_IidToEntity.Clear();
        }
    }
}
