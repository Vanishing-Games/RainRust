namespace Core
{
    public readonly struct LoadContext
    {
        public readonly GameFlowState Destination;
        public readonly string ChapterId;
        public readonly string LevelId;
        public readonly int SpawnIndex;
        public readonly string SavePointName;
        public readonly bool IsStandalone;

        private LoadContext(
            GameFlowState destination,
            string chapterId,
            string levelId,
            int spawnIndex,
            string savePointName,
            bool isStandalone
        )
        {
            Destination = destination;
            ChapterId = chapterId;
            LevelId = levelId;
            SpawnIndex = spawnIndex;
            SavePointName = savePointName;
            IsStandalone = isStandalone;
        }

        public static LoadContext ForLevel(string chapterId, string levelId, int spawnIndex) =>
            new(GameFlowState.InLevel, chapterId, levelId, spawnIndex, null, false);

        public static LoadContext ForSavePoint(string savePointName) =>
            new(GameFlowState.InLevel, null, null, 0, savePointName, false);

        public static LoadContext ForMainMenu() =>
            new(GameFlowState.MainMenu, null, null, 0, null, false);

        public static LoadContext ForStandalone(string chapterId, string levelId, int spawnIndex) =>
            new(GameFlowState.InLevel, chapterId, levelId, spawnIndex, null, true);
    }
}
