using IngameDebugConsole;

namespace GameMain.RunTime
{
    public static class LevelConsoleCommands
    {
        [ConsoleMethod("start_level", "Directly start a level by save point name")]
        public static void StartLevel(string savePointName)
        {
            new StartLevelCommand(savePointName).Execute();
        }

        [ConsoleMethod(
            "start_level_by_index",
            "Directly start a level by chapterId, levelId and spawnIndex"
        )]
        public static void StartLevelByIndex(string chapterId, string levelId, int spawnIndex)
        {
            LevelManager.Instance.StartLevel(chapterId, levelId, spawnIndex);
        }

        [ConsoleMethod("load_level", "Load a level via LoadManager with pipeline process")]
        public static void LoadLevel(string chapterId, string levelId, int spawnIndex)
        {
            new LoadLevelCommand(chapterId, levelId, spawnIndex).Execute();
        }

        [ConsoleMethod("end_level", "End current level")]
        public static void EndLevel()
        {
            new EndLevelCommand().Execute();
        }
    }
}
