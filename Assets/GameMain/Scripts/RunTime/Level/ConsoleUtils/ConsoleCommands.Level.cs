using IngameDebugConsole;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class LevelConsoleCommands
    {
        [ConsoleMethod(
            "start_level",
            "Directly start a level by chapterId, levelId and spawnIndex"
        )]
        public static void StartLevel(string chapterId, string levelId, int spawnIndex)
        {
            new StartLevelCommand(chapterId, levelId, spawnIndex).Execute();
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

        [ConsoleMethod("switch_level", "Switch current level")]
        public static void SwitchLevel(string chapterId, string levelId, int spawnIndex)
        {
            new SwitchLevelCommand(chapterId, levelId, spawnIndex).Execute();
        }
    }
}
