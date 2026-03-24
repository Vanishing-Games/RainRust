using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class StatsSaveData : ISaveData
    {
        public List<StatRecord> Stats = new();
    }
}
