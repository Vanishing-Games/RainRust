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

    public enum SaveMode
    {
        Editor,
        Runtime,
    }

    public enum RootPathType
    {
        PersistentDataPath,
        DataPathRelative,
    }

    [Serializable]
    public class PlatformPathInfo
    {
        [TableColumnWidth(120, false)]
        [ReadOnly]
        public string Platform;

        [ReadOnly]
        public string Path;

        public PlatformPathInfo(string platform, string path)
        {
            Platform = platform;
            Path = path;
        }
    }

    public class SaveManager : MonoSingletonPersistent<SaveManager>
    {
        protected override void Awake()
        {
            base.Awake();
            InitializeDirectory();
            RefreshSaveSlots();
            UpdatePlatformPathPreviews();
        }

        [Button("Save To Current Slot", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1)]
        [BoxGroup("Actions")]
        public void SaveCurrent()
        {
            Save(m_SelectedSlot, m_SelectedSlot);
        }

        [Button("Load Selected Slot", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f)]
        [BoxGroup("Actions")]
        [EnableIf("@!string.IsNullOrEmpty(m_SelectedSlot)")]
        public void LoadSelected()
        {
            Load(m_SelectedSlot);
        }

        [Button("Delete Selected Slot", ButtonSizes.Small), GUIColor(1f, 0.4f, 0.4f)]
        [BoxGroup("Actions")]
        [EnableIf("@!string.IsNullOrEmpty(m_SelectedSlot)")]
        public void DeleteSelected()
        {
            string fullPath = GetSavePath(m_SelectedSlot);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                CLogger.LogInfo($"Deleted save slot: {m_SelectedSlot}", LogTag.Game);
                RefreshSaveSlots();
            }
        }

        [Button("Save As New Slot", ButtonSizes.Small)]
        [BoxGroup("Actions/New Save")]
        public void SaveNew(string newSlotName, string displayName = "")
        {
            if (string.IsNullOrEmpty(newSlotName))
                return;
            Save(newSlotName, displayName);
            m_SelectedSlot = newSlotName;
        }

        public void Save(string slotName, string displayName = "")
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

                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(container, settings);
                File.WriteAllText(tempPath, json);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
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

        public void Load(string slotName)
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
            string dir = SaveDirectory;
            if (!Directory.Exists(dir))
                return;

            var files = Directory.GetFiles(dir, $"*{m_Extension}");
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

        private IEnumerable<string> GetSlotNames()
        {
            // Auto refresh when opening dropdown in Editor
            if (!Application.isPlaying)
                RefreshSaveSlots();

            if (m_AvailableSlots == null || m_AvailableSlots.Count == 0)
                return new[] { "default" };

            return m_AvailableSlots.Select(x => x.SlotName);
        }

        [OnInspectorGUI]
        private void UpdatePlatformPathPreviews()
        {
            m_PlatformPreviews.Clear();
            m_PlatformPreviews.Add(
                new PlatformPathInfo(
                    "Windows",
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "..",
                        "LocalLow",
                        Application.companyName,
                        Application.productName,
                        m_SaveFolderRelativePath
                    )
                )
            );
            m_PlatformPreviews.Add(
                new PlatformPathInfo(
                    "macOS",
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                        "Library",
                        "Application Support",
                        Application.companyName,
                        Application.productName,
                        m_SaveFolderRelativePath
                    )
                )
            );
            m_PlatformPreviews.Add(
                new PlatformPathInfo(
                    "Android",
                    "/storage/emulated/0/Android/data/"
                        + Application.identifier
                        + "/files/"
                        + m_SaveFolderRelativePath
                )
            );
            m_PlatformPreviews.Add(
                new PlatformPathInfo(
                    "iOS",
                    "Data/Containers/Data/Application/.../Library/Application Support/"
                        + m_SaveFolderRelativePath
                )
            );
        }

        private void InitializeDirectory()
        {
            string dir = SaveDirectory;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private string GetSavePath(string slotName)
        {
            return Path.Combine(SaveDirectory, $"{slotName}{m_Extension}");
        }

        [ShowInInspector, ReadOnly]
        [BoxGroup("Status")]
        [InfoBox("Current Active Path: $SaveDirectory")]
        public string SaveDirectory
        {
            get
            {
                if (m_SaveMode == SaveMode.Editor)
                {
                    return Path.Combine(Directory.GetCurrentDirectory(), "SaveData_Editor");
                }

                string root = m_RootPathType switch
                {
                    RootPathType.PersistentDataPath => Application.persistentDataPath,
                    RootPathType.DataPathRelative => Path.Combine(Application.dataPath, ".."),
                    _ => Application.persistentDataPath,
                };
                return Path.Combine(root, m_SaveFolderRelativePath);
            }
        }

        [Header("Slot Selection")]
        [SerializeField]
        [BoxGroup("Actions")]
        [ValueDropdown("GetSlotNames")]
        private string m_SelectedSlot = "default";

        [Header("Mode Configuration")]
        [SerializeField]
        [BoxGroup("Configuration")]
        [EnumToggleButtons]
        private SaveMode m_SaveMode = SaveMode.Editor;

        [SerializeField]
        [BoxGroup("Configuration")]
        private RootPathType m_RootPathType = RootPathType.PersistentDataPath;

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

        [SerializeField, ReadOnly]
        [BoxGroup("Platform Previews")]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        private List<PlatformPathInfo> m_PlatformPreviews = new();

        private static SaveManager m_Instance;
        private readonly List<ISavable> m_Savables = new();
    }
}
