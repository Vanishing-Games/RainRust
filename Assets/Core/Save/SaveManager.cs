using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    public struct SaveEvent : IEvent
    {
        public SaveEvent(string slot, bool success)
        {
            Slot = slot;
            Success = success;
        }

        public string Slot { get; }
        public bool Success { get; }
    }

    public struct LoadEvent : IEvent
    {
        public LoadEvent(string slot, bool success)
        {
            Slot = slot;
            Success = success;
        }

        public string Slot { get; }
        public bool Success { get; }
    }

    public class SaveManager : MonoSingletonPersistent<SaveManager>
    {
        protected override void Awake()
        {
            base.Awake();
            InitializeDirectory();
            RefreshSaveSlots();
        }

        [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
        [BoxGroup("Actions")]
        public void Save(string slotName = "default", string displayName = "")
        {
            string fullPath = GetSavePath(slotName);
            string tempPath = fullPath + ".tmp";

            try
            {
                var container = new SaveContainer();
                container.Meta = new SaveMeta(slotName)
                {
                    DisplayName = string.IsNullOrEmpty(displayName)
                        ? $"Save_{slotName}"
                        : displayName,
                    LastSavedTime = DateTime.Now,
                    PlayTimeInSeconds = StatsManager.GetValue(StatKeys.GameDuration),
                };

                foreach (var savable in m_Savables)
                {
                    container.Data[savable.SaveID] = savable.CaptureState();
                }

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                string json = JsonConvert.SerializeObject(container, settings);
                File.WriteAllText(tempPath, json);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                File.Move(tempPath, fullPath);

                m_CurrentSlot = slotName;
                RefreshSaveSlots();

                CLogger.LogInfo($"Game saved to {fullPath}", LogTag.Game);
                MessageBroker.Global.Publish(new SaveEvent(slotName, true));
            }
            catch (Exception e)
            {
                CLogger.LogError($"Save failed: {e.Message}", LogTag.Game);
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                MessageBroker.Global.Publish(new SaveEvent(slotName, false));
            }
        }

        [Button(ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        [BoxGroup("Actions")]
        public void Load(string slotName = "default")
        {
            string fullPath = GetSavePath(slotName);
            if (!File.Exists(fullPath))
            {
                CLogger.LogWarn($"Save file not found: {fullPath}", LogTag.Game);
                return;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                var container = JsonConvert.DeserializeObject<SaveContainer>(json, settings);
                if (container == null)
                    return;

                foreach (var savable in m_Savables)
                {
                    if (container.Data.TryGetValue(savable.SaveID, out var state))
                    {
                        savable.RestoreState(state);
                    }
                }

                m_CurrentSlot = slotName;
                CLogger.LogInfo($"Game loaded from {fullPath}", LogTag.Game);
                MessageBroker.Global.Publish(new LoadEvent(slotName, true));
            }
            catch (Exception e)
            {
                CLogger.LogError($"Load failed: {e.Message}", LogTag.Game);
                MessageBroker.Global.Publish(new LoadEvent(slotName, false));
            }
        }

        public void Register(ISavable savable)
        {
            if (string.IsNullOrEmpty(savable.SaveID))
            {
                CLogger.LogWarn($"Savable {savable.GetType().Name} has no SaveID!", LogTag.Game);
                return;
            }

            if (!m_Savables.Contains(savable))
            {
                m_Savables.Add(savable);
            }
        }

        public void Unregister(ISavable savable)
        {
            m_Savables.Remove(savable);
        }

        [Button]
        [BoxGroup("Slot Management")]
        public void RefreshSaveSlots()
        {
            m_AvailableSlots.Clear();
            if (!Directory.Exists(SaveDirectory))
                return;

            var files = Directory.GetFiles(SaveDirectory, $"*{m_Extension}");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var container = JsonConvert.DeserializeObject<SaveContainer>(json);
                    if (container?.Meta != null)
                    {
                        m_AvailableSlots.Add(container.Meta);
                    }
                }
                catch (Exception e)
                {
                    CLogger.LogWarn(
                        $"Failed to read save meta from {file}: {e.Message}",
                        LogTag.Game
                    );
                }
            }
        }

        private void InitializeDirectory()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        private string GetSavePath(string slotName)
        {
            return Path.Combine(SaveDirectory, $"{slotName}{m_Extension}");
        }

        [ShowInInspector, ReadOnly]
        [BoxGroup("Status")]
        public string SaveDirectory =>
            Path.Combine(Application.persistentDataPath, m_SaveFolderRelativePath);

        [Header("Settings")]
        [SerializeField]
        [BoxGroup("Configuration")]
        private string m_SaveFolderRelativePath = "Saves";

        [SerializeField]
        [BoxGroup("Configuration")]
        private string m_Extension = ".json";

        [SerializeField, ReadOnly]
        [BoxGroup("Status")]
        private string m_CurrentSlot = "default";

        [SerializeField, ReadOnly]
        [BoxGroup("Slot Management")]
        [TableList]
        private List<SaveMeta> m_AvailableSlots = new();

        private readonly List<ISavable> m_Savables = new();
    }
}
