using Core;

namespace GameMain.RunTime
{
    public static class SnakeGameEvents
    {
        public struct SnakeDeathEvent : IEvent { }
        public struct SnakeReachedSavePointEvent : IEvent { }
    }
}
