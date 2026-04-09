using Core;

namespace GameMain.RunTime
{
    public static class LevelManagerEvents
    {
        public enum LevelSwitchDirection
        {
            Horizontal,
            Vertical,
        }

        public readonly struct LevelSwitchedEvent : IEvent
        {
            public LevelSwitchedEvent(LevelSwitchDirection switchDirection) =>
                Direction = switchDirection;

            public LevelSwitchDirection Direction { get; }
        }
    }
}
