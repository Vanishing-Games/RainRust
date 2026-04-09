namespace Core
{
    public static class GameCoreEvents
    {
        public struct GameCorePreInitEvent : IEvent { }

        public struct GameCorePostInitEvent : IEvent { }

        public struct GameCoreLevelPreInitEvent : IEvent { }

        public struct GameCorePostEndEvent : IEvent { }

        public struct GameCoreQuickStartEvent : IEvent { }

        public abstract class GameCoreCustomInitEvent : IEvent { }
    }
}
