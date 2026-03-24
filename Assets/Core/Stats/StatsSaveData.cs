using System.Collections.Generic;

namespace Core.Stats
{
    [System.Serializable]
    public class StatsSaveData : ISaveData
    {
        public List<StatRecord> Stats = new();
    }
}
