namespace Core
{
    public interface IFmodParameterEvent : IEvent
    {
        string ManagedId { get; }
        string ParameterName { get; }
        float Value { get; }
    }
}
