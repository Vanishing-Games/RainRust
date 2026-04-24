using Core;

namespace GameMain.RunTime
{
    public static class GamePlaySnakeGameEvents
    {
        public struct SnakeDeathEvent : IEvent { }

        public struct SnakeSaveEvent : IEvent { }

        public struct HoneyCollectedEvent : IEvent
        {
            public SnakeHoney Honey;
        }

        public struct HoneyResetEvent : IEvent
        {
            public SnakeHoney Honey;
        }
    }
}
