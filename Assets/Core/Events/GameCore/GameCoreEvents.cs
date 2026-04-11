namespace Core
{
    public static class GameCoreEvents
    {
        public struct LevelClearEvent : IEvent
        {
            public string NextChapterId;
            public string NextLevelId;

            public LevelClearEvent(string nextChapterId, string nextLevelId)
            {
                NextChapterId = nextChapterId;
                NextLevelId = nextLevelId;
            }
        }
    }
}
