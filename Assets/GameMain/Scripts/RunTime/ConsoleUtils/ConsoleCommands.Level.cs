using IngameDebugConsole;

namespace GameMain.RunTime
{
    public static class LevelConsoleCommands
    {
        [ConsoleMethod("start_level", "Directly start a level by save point name")]
        public static void StartLevel(string savePointName)
        {
            new LevelManagerCommands.StartLevelCommand(savePointName).Execute();
        }

        [ConsoleMethod("start_level_by_id", "Directly start a level by chapterId and levelId")]
        public static void StartLevelById(string chapterId, string levelId)
        {
            LevelManager.Instance.StartLevel(chapterId, levelId);
        }

        [ConsoleMethod("load_level", "Load a level via LoadManager with pipeline process")]
        public static void LoadLevel(string chapterId, string levelId)
        {
            new LevelManagerCommands.LoadLevelCommand(chapterId, levelId).Execute();
        }

        [ConsoleMethod("end_level", "End current level")]
        public static void EndLevel()
        {
            new LevelManagerCommands.EndLevelCommand().Execute();
        }
    }
}
