using Core;

namespace GameMain.RunTime
{
    public class EndLevelCommand : ITriggerCommand
    {
        public bool Execute()
        {
            LevelManager.Instance.EndLevel();
            return true;
        }
    }
}
