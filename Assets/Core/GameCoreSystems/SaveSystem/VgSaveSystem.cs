using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
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

    public class VgSaveSystem : CoreModuleManagerBase<VgSaveSystem>, ICoreModuleSystem
    {
        public string SystemName => "VgSaveSystem";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnBootStart(async () =>
            {
                InitializeDirectory();
                RefreshSaveSlots();
                m_SaveRequestSubscription =
                    MessageBroker.Global.Subscribe<SaveSystemEvents.SaveRequestEvent>(
                        OnSaveRequested
                    );
                await LoadGlobalSaveAsync();
            });

            registry.OnGameQuit(async () =>
            {
                await WriteSlotSaveAsync();
                await WriteGlobalSaveAsync();
                m_SaveRequestSubscription?.Dispose();
            });
        }

        public void UpdateSaveValue<T>(string key, T value, bool isGlobal = false)
        {
            var container = isGlobal ? m_GlobalContainer : m_CurrentSlotContainer;
            container.Data[key] = value;

            if (isGlobal)
            {
                m_IsGlobalDirty = true;
            }
            else
            {
                m_IsSlotDirty = true;
            }

            MessageBroker.Global.Publish(
                new SaveSystemEvents.SaveValueUpdatedEvent(key, value, isGlobal)
            );
        }

        public T GetSaveValue<T>(string key, T defaultValue = default, bool isGlobal = false)
        {
            var container = isGlobal ? m_GlobalContainer : m_CurrentSlotContainer;
            if (container.Data.TryGetValue(key, out var val))
            {
                if (val is T typedVal)
                {
                    return typedVal;
                }

                try
                {
                    string json = JsonConvert.SerializeObject(val);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public bool HasKey(string key, bool isGlobal = false)
        {
            var container = isGlobal ? m_GlobalContainer : m_CurrentSlotContainer;
            return container.Data.ContainsKey(key);
        }

        [Button("Save Current Slot", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1)]
        [BoxGroup("Actions")]
        public async UniTask<bool> WriteSlotSaveAsync()
        {
            return await WriteSaveFileAsync(m_CurrentSlot, false);
        }

        [Button("Save Global", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
        [BoxGroup("Actions")]
        public async UniTask<bool> WriteGlobalSaveAsync()
        {
            return await WriteSaveFileAsync(m_GlobalSaveName, true);
        }

        [Button("Load Selected Slot", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f)]
        [BoxGroup("Actions")]
        [EnableIf("@!string.IsNullOrEmpty(m_SelectedSlot)")]
        public async UniTask<bool> LoadSelectedSlotAsync()
        {
            return await LoadSaveFileAsync(m_SelectedSlot, false);
        }

        private async void OnSaveRequested(SaveSystemEvents.SaveRequestEvent evt)
        {
            if (evt.IsGlobal)
            {
                await WriteGlobalSaveAsync();
            }
            else
            {
                await WriteSaveFileAsync(evt.SlotName ?? m_CurrentSlot, false);
            }
        }

        private async UniTask<bool> WriteSaveFileAsync(string slotName, bool isGlobal)
        {
            MessageBroker.Global.Publish(
                new SaveSystemEvents.SavePreWriteEvent(slotName, isGlobal)
            );

            string fullPath = GetSavePath(slotName);
            string tempPath = fullPath + ".tmp";
            var container = isGlobal ? m_GlobalContainer : m_CurrentSlotContainer;

            try
            {
                container.Meta.SlotName = slotName;
                container.Meta.LastSavedTime = DateTime.Now;
                if (!isGlobal)
                {
                    container.Meta.PlayTimeInSeconds = StatsManager.GetValue(StatKeys.GameDuration);
                }

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                string json = await UniTask.RunOnThreadPool(() =>
                    JsonConvert.SerializeObject(container, settings)
                );
                await File.WriteAllTextAsync(tempPath, json);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                File.Move(tempPath, fullPath);

                if (isGlobal)
                {
                    m_IsGlobalDirty = false;
                }
                else
                {
                    m_IsSlotDirty = false;
                    m_CurrentSlot = slotName;
                    RefreshSaveSlots();
                }

                CLogger.LogInfo(
                    $"{(isGlobal ? "Global" : "Slot")} save success: {fullPath}",
                    LogTag.Game
                );
                MessageBroker.Global.Publish(
                    new SaveSystemEvents.SavePostWriteEvent(slotName, true, isGlobal)
                );
                return true;
            }
            catch (Exception e)
            {
                CLogger.LogError($"Save failed: {e.Message}", LogTag.Game);
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                MessageBroker.Global.Publish(
                    new SaveSystemEvents.SavePostWriteEvent(slotName, false, isGlobal)
                );
                return false;
            }
        }

        private async UniTask<bool> LoadSaveFileAsync(string slotName, bool isGlobal)
        {
            string fullPath = GetSavePath(slotName);
            if (!File.Exists(fullPath))
            {
                CLogger.LogWarn($"Save file not found: {fullPath}", LogTag.Game);
                return false;
            }

            MessageBroker.Global.Publish(new SaveSystemEvents.SavePreLoadEvent(slotName, isGlobal));

            try
            {
                string json = await File.ReadAllTextAsync(fullPath);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                var container = await UniTask.RunOnThreadPool(() =>
                    JsonConvert.DeserializeObject<SaveContainer>(json, settings)
                );
                if (container == null)
                {
                    return false;
                }

                if (isGlobal)
                {
                    m_GlobalContainer = container;
                }
                else
                {
                    m_CurrentSlotContainer = container;
                    m_CurrentSlot = slotName;
                }

                MessageBroker.Global.Publish(
                    new SaveSystemEvents.SaveOnLoadEvent(container, isGlobal)
                );

                CLogger.LogInfo(
                    $"{(isGlobal ? "Global" : "Slot")} load success: {fullPath}",
                    LogTag.Game
                );
                MessageBroker.Global.Publish(
                    new SaveSystemEvents.SavePostLoadEvent(slotName, true, isGlobal)
                );
                return true;
            }
            catch (Exception e)
            {
                CLogger.LogError($"Load failed: {e.Message}", LogTag.Game);
                MessageBroker.Global.Publish(
                    new SaveSystemEvents.SavePostLoadEvent(slotName, false, isGlobal)
                );
                return false;
            }
        }

        private async UniTask LoadGlobalSaveAsync()
        {
            if (File.Exists(GetSavePath(m_GlobalSaveName)))
            {
                await LoadSaveFileAsync(m_GlobalSaveName, true);
            }
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

        [Button("Refresh Slots")]
        [BoxGroup("Slot Management")]
        public void RefreshSaveSlots()
        {
            m_AvailableSlots.Clear();
            string dir = SaveDirectory;
            if (!Directory.Exists(dir))
            {
                return;
            }

            var files = Directory.GetFiles(dir, $"*{m_Extension}");
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName == m_GlobalSaveName)
                {
                    continue;
                }

                try
                {
                    string json = File.ReadAllText(file);
                    var container = JsonConvert.DeserializeObject<SaveContainer>(json);
                    if (container?.Meta != null)
                    {
                        m_AvailableSlots.Add(container.Meta);
                    }
                }
                catch { }
            }
        }

        private IEnumerable<string> GetSlotNames()
        {
            if (!Application.isPlaying)
            {
                RefreshSaveSlots();
            }

            if (m_AvailableSlots == null || m_AvailableSlots.Count == 0)
            {
                return new[] { "default" };
            }

            return m_AvailableSlots.Select(x => x.SlotName);
        }

        public string CurrentSlotName => m_CurrentSlot;
        public bool IsSlotDirty => m_IsSlotDirty;
        public bool IsGlobalDirty => m_IsGlobalDirty;

        public void SetCurrentSlot(string slotName)
        {
            m_CurrentSlot = slotName;
            m_CurrentSlotContainer = new SaveContainer();
            m_CurrentSlotContainer.Meta = new SaveMeta(slotName);
            m_CurrentSlotContainer.Meta.DisplayName = slotName;
            m_IsSlotDirty = true;
        }

        public async UniTask<bool> RenameSlotAsync(string slotName, string newDisplayName)
        {
            string fullPath = GetSavePath(slotName);
            if (!File.Exists(fullPath))
                return false;

            try
            {
                string json = await File.ReadAllTextAsync(fullPath);
                var container = JsonConvert.DeserializeObject<SaveContainer>(json);
                if (container == null)
                    return false;

                container.Meta.DisplayName = newDisplayName;

                string newJson = JsonConvert.SerializeObject(container, Formatting.Indented);
                await File.WriteAllTextAsync(fullPath, newJson);

                RefreshSaveSlots();
                return true;
            }
            catch (Exception e)
            {
                CLogger.LogError($"Rename failed: {e.Message}", LogTag.Game);
                return false;
            }
        }

        [BoxGroup("Status")]
        [ShowInInspector, ReadOnly]
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

        [ShowInInspector, ReadOnly, TabGroup("Runtime Status")]
        private SaveContainer m_CurrentSlotContainer = new();

        [ShowInInspector, ReadOnly, TabGroup("Runtime Status")]
        private SaveContainer m_GlobalContainer = new();

        [ShowInInspector, ReadOnly, TabGroup("Runtime Status")]
        private bool m_IsSlotDirty;

        [ShowInInspector, ReadOnly, TabGroup("Runtime Status")]
        private bool m_IsGlobalDirty;

        [SerializeField, BoxGroup("Settings")]
        private SaveMode m_SaveMode = SaveMode.Editor;

        [SerializeField, BoxGroup("Settings")]
        private RootPathType m_RootPathType = RootPathType.PersistentDataPath;

        [SerializeField, BoxGroup("Settings")]
        private string m_SaveFolderRelativePath = "Saves";

        [SerializeField, BoxGroup("Settings")]
        private string m_GlobalSaveName = "global_settings";

        [SerializeField, BoxGroup("Settings")]
        private string m_Extension = ".json";

        public IReadOnlyList<SaveMeta> AvailableSlots => m_AvailableSlots;

        [SerializeField, ReadOnly, BoxGroup("Status")]
        private string m_CurrentSlot = "default";

        [SerializeField, ReadOnly, BoxGroup("Slot Management")]
        [TableList]
        private List<SaveMeta> m_AvailableSlots = new();

        public async UniTask<bool> LoadSlotAsync(string slotName)
        {
            return await LoadSaveFileAsync(slotName, false);
        }

        [SerializeField]
        [BoxGroup("Actions")]
        [ValueDropdown("GetSlotNames")]
        private string m_SelectedSlot = "default";

        private IDisposable m_SaveRequestSubscription;
    }
}
