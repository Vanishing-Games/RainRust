using Core;

namespace GameMain.RunTime
{
    public class StartLevelCommand : ITriggerCommand
    {
        private readonly string m_SavePointName;

        public StartLevelCommand(string savePointName)
        {
            m_SavePointName = savePointName;
        }

        public bool Execute()
        {
            LevelManager.Instance.StartLevel(m_SavePointName);
            return true;
        }
    }
}
