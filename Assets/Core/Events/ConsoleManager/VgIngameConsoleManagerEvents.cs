namespace Core
{
    public static class VgIngameConsoleManagerEvents
    {
        public readonly struct ConsoleVisibilityChangedEvent : IEvent
        {
            public ConsoleVisibilityChangedEvent(bool isVisible) => IsVisible = isVisible;

            public bool IsVisible { get; }
        }

        public readonly struct ConsoleDisplayModeChangedEvent : IEvent
        {
            public ConsoleDisplayModeChangedEvent(ConsoleDisplayMode mode) => Mode = mode;

            public ConsoleDisplayMode Mode { get; }
        }
    }
}
