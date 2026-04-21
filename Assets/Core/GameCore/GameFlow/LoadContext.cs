namespace Core
{
    public readonly struct LoadContext
    {
        public readonly GameFlowState Destination;
        public readonly string ChapterId;
        public readonly string LevelId;
        public readonly string SavePointName;
        public readonly bool IsStandalone;

        private LoadContext(
            GameFlowState destination,
            string chapterId,
            string levelId,
            string savePointName,
            bool isStandalone
        )
        {
            Destination = destination;
            ChapterId = chapterId;
            LevelId = levelId;
            SavePointName = savePointName;
            IsStandalone = isStandalone;
        }

        public static LoadContext ForLevel(string chapterId, string levelId) =>
            new(GameFlowState.InLevel, chapterId, levelId, null, false);

        public static LoadContext ForSavePoint(string savePointName) =>
            new(GameFlowState.InLevel, null, null, savePointName, false);

        public static LoadContext ForMainMenu() =>
            new(GameFlowState.MainMenu, null, null, null, false);

        public static LoadContext ForStandalone(string chapterId, string levelId) =>
            new(GameFlowState.InLevel, chapterId, levelId, null, true);
    }
}
