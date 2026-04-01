using Core;

namespace GameMain.RunTime
{
    public class LevelSwitchEvent : IEvent
    {
        public enum LevelSwitchDirection
        {
            Horizontal,
            Vertical,
        }

        public LevelSwitchEvent(LevelSwitchDirection switchDirection) =>
            Direction = switchDirection;

        public LevelSwitchDirection Direction { get; private set; }
    }
}
