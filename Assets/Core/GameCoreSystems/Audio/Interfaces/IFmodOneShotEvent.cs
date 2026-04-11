using FMODUnity;

namespace Core
{
    public interface IFmodOneShotEvent : IEvent
    {
        EventReference FmodEvent { get; }
    }
}
