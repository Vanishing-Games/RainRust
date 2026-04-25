namespace Core
{
    public static class GameCoreEvents
    {
        public struct LevelClearEvent : IEvent
        {
            public string NextSavePointName;

            public LevelClearEvent(string nextSavePointName)
            {
                NextSavePointName = nextSavePointName;
            }
        }
    }
}
