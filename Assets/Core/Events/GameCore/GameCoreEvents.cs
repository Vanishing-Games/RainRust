namespace Core
{
    public static class GameCoreEvents
    {
        public struct GameCoreQuickStartEvent : IEvent { }

        public abstract class GameCoreCustomInitEvent : IEvent { }
    }
}
