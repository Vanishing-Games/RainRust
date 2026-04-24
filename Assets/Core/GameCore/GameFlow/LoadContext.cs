namespace Core
{
    public readonly struct LoadContext
    {
        public readonly GameFlowState Destination;
        public readonly string ChapterId;
        public readonly string LevelId;
        public readonly string SavePointName;

        private LoadContext(
            GameFlowState destination,
            string chapterId,
            string levelId,
            string savePointName
        )
        {
            Destination = destination;
            ChapterId = chapterId;
            LevelId = levelId;
            SavePointName = savePointName;
        }

        public static LoadContext ForLevel(string chapterId, string levelId) =>
            new(GameFlowState.InLevel, chapterId, levelId, null);

        public static LoadContext ForSavePoint(string savePointName) =>
            new(GameFlowState.InLevel, null, null, savePointName);

        public static LoadContext ForMainMenu() => new(GameFlowState.MainMenu, null, null, null);
    }
}
