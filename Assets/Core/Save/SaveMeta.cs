using System;

namespace Core
{
    [Serializable]
    public class SaveMeta
    {
        public SaveMeta(string slotName)
        {
            SlotName = slotName;
            LastSavedTime = DateTime.Now;
        }

        public string SlotName;
        public string DisplayName;
        public double PlayTimeInSeconds;
        public DateTime LastSavedTime;
        public string SaveFileVersion = "1.0.0";
    }
}
