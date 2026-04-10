using System;

namespace Core
{
    public static class SaveSystemEvents
    {
        public interface ISaveEvent : IEvent { }

        public struct SavePreWriteEvent : ISaveEvent
        {
            public string SlotName;
            public bool IsGlobal;

            public SavePreWriteEvent(string slotName, bool isGlobal = false)
            {
                SlotName = slotName;
                IsGlobal = isGlobal;
            }
        }

        public struct SavePostWriteEvent : ISaveEvent
        {
            public string SlotName;
            public bool IsGlobal;
            public bool Success;

            public SavePostWriteEvent(string slotName, bool success, bool isGlobal = false)
            {
                SlotName = slotName;
                Success = success;
                IsGlobal = isGlobal;
            }
        }

        public struct SavePreLoadEvent : ISaveEvent
        {
            public string SlotName;
            public bool IsGlobal;

            public SavePreLoadEvent(string slotName, bool isGlobal = false)
            {
                SlotName = slotName;
                IsGlobal = isGlobal;
            }
        }

        public struct SaveOnLoadEvent : ISaveEvent
        {
            public SaveContainer Container;
            public bool IsGlobal;

            public SaveOnLoadEvent(SaveContainer container, bool isGlobal = false)
            {
                Container = container;
                IsGlobal = isGlobal;
            }
        }

        public struct SavePostLoadEvent : ISaveEvent
        {
            public string SlotName;
            public bool IsGlobal;
            public bool Success;

            public SavePostLoadEvent(string slotName, bool success, bool isGlobal = false)
            {
                SlotName = slotName;
                Success = success;
                IsGlobal = isGlobal;
            }
        }

        public struct SaveValueUpdatedEvent : ISaveEvent
        {
            public string Key;
            public object Value;
            public bool IsGlobal;

            public SaveValueUpdatedEvent(string key, object value, bool isGlobal = false)
            {
                Key = key;
                Value = value;
                IsGlobal = isGlobal;
            }
        }

        public struct SaveRequestEvent : ISaveEvent
        {
            public string SlotName;
            public bool IsGlobal;

            public SaveRequestEvent(string slotName = null, bool isGlobal = false)
            {
                SlotName = slotName;
                IsGlobal = isGlobal;
            }
        }
    }
}
