namespace Core
{
    public interface IFloatValueEvent : IEvent
    {
        float Value { get; }
    }

    public interface IStringValueEvent : IEvent
    {
        string Value { get; }
    }
}
