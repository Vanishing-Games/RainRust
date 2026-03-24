namespace Core.Stats
{
    public enum StatType
    {
        Counter,
        Timer,
        Max,
    }

    [System.Serializable]
    public class StatRecord
    {
        public StatRecord(string key, StatType type, string displayName = "")
        {
            Key = key;
            Type = type;
            Value = 0;
            DisplayName = string.IsNullOrEmpty(displayName) ? key : displayName;
        }

        public string Key;
        public float Value;
        public StatType Type;
        public string DisplayName;
    }
}
