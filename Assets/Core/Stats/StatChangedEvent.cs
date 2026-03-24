namespace Core
{
    public struct StatChangedEvent : IEvent
    {
        public StatChangedEvent(string key, float oldValue, float newValue, StatType type)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
            Type = type;
        }

        public string Key { get; }
        public float OldValue { get; }
        public float NewValue { get; }
        public StatType Type { get; }
    }
}
