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

        public readonly struct LevelManagerPreEnterChapterEvent : IStringValueEvent
        {
            public LevelManagerPreEnterChapterEvent(string chapterName) => Value = chapterName;

            public string Value { get; }
        }

        public readonly struct LevelManagerPostEnterChapterEvent : IStringValueEvent
        {
            public LevelManagerPostEnterChapterEvent(string chapterName) => Value = chapterName;

            public string Value { get; }
        }

        public readonly struct LevelManagerPreExitChapterEvent : IStringValueEvent
        {
            public LevelManagerPreExitChapterEvent(string chapterName) => Value = chapterName;

            public string Value { get; }
        }

        public readonly struct LevelManagerPostExitChapterEvent : IStringValueEvent
        {
            public LevelManagerPostExitChapterEvent(string chapterName) => Value = chapterName;

            public string Value { get; }
        }

        public readonly struct LevelManagerLevelSwitchedEvent : IEvent
        {
            public LevelManagerLevelSwitchedEvent(LevelSwitchDirection switchDirection) =>
                Direction = switchDirection;

            public LevelSwitchDirection Direction { get; }
        }
    }
}
