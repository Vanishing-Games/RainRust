using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Save
{
    public class SaveManager : MonoBehaviour
    {
        private void Awake()
        {
            if (m_Instance != null && m_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            m_Instance = this;

            if (m_DontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            m_SaveDirectory = Path.Combine(Application.persistentDataPath, m_SaveFolderName);
            if (!Directory.Exists(m_SaveDirectory))
            {
                Directory.CreateDirectory(m_SaveDirectory);
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

        public void Save(string slotName = "default")
        {
            string fullPath = GetSavePath(slotName);
            string tempPath = fullPath + ".tmp";

            try
            {
                var saveData = new Dictionary<string, object>();
                foreach (var savable in m_Savables)
                {
                    saveData[savable.SaveID] = savable.CaptureState();
                }

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                string json = JsonConvert.SerializeObject(saveData, settings);
                File.WriteAllText(tempPath, json);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                File.Move(tempPath, fullPath);

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

                var saveData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    json,
                    settings
                );
                if (saveData == null)
                    return;

                foreach (var savable in m_Savables)
                {
                    if (saveData.TryGetValue(savable.SaveID, out var state))
                    {
                        // JSON deserializes sub-objects as JObject/JArray by default if not typed.
                        // TypeNameHandling.Auto helps, but we might need to handle casting carefully.
                        // To be robust, we convert back to JSON and then to the target type if needed,
                        // or rely on Newtonsoft's internal conversion.
                        savable.RestoreState(state);
                    }
                }

                CLogger.LogInfo($"Game loaded from {fullPath}", LogTag.Game);
                MessageBroker.Global.Publish(new LoadEvent(slotName, true));
            }
            catch (Exception e)
            {
                CLogger.LogError($"Load failed: {e.Message}", LogTag.Game);
                MessageBroker.Global.Publish(new LoadEvent(slotName, false));
            }
        }

        public void DeleteSave(string slotName = "default")
        {
            string fullPath = GetSavePath(slotName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                CLogger.LogInfo($"Deleted save slot: {slotName}", LogTag.Game);
            }
        }

        private string GetSavePath(string slotName)
        {
            return Path.Combine(m_SaveDirectory, $"{slotName}{m_Extension}");
        }

        public static SaveManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindFirstObjectByType<SaveManager>();
                    if (m_Instance == null)
                    {
                        GameObject go = new GameObject("SaveManager");
                        m_Instance = go.AddComponent<SaveManager>();
                    }
                }
                return m_Instance;
            }
        }

        [Header("Settings")]
        [SerializeField]
        private bool m_DontDestroyOnLoad = true;

        [SerializeField]
        private string m_SaveFolderName = "Saves";

        [SerializeField]
        private string m_Extension = ".json";

        private static SaveManager m_Instance;
        private string m_SaveDirectory;
        private readonly List<ISavable> m_Savables = new();
    }

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
}
