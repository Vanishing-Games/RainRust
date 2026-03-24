using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core
{
    public class StatsManager : MonoBehaviour, ISavable<StatsSaveData>
    {
        private void Awake()
        {
            if (m_Instance != null && m_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            m_Instance = this;

            foreach (var stat in m_InitialStats)
            {
                if (!m_Stats.ContainsKey(stat.Key))
                {
                    m_Stats.Add(stat.Key, stat);
                }
            }

            CacheTimerKeys();
        }

        private void Start()
        {
            SaveManager.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (m_Instance == this && SaveManager.Instance != null)
            {
                SaveManager.Instance.Unregister(this);
            }
        }

        private void Update()
        {
            foreach (var key in m_CachedTimerKeys)
            {
                UpdateStat(key, m_Stats[key].Value + Time.deltaTime, false);
            }
        }

        public StatsSaveData CaptureSaveData()
        {
            return new StatsSaveData { Stats = m_Stats.Values.ToList() };
        }

        public void RestoreSaveData(StatsSaveData data)
        {
            if (data?.Stats == null)
                return;

            foreach (var record in data.Stats)
            {
                if (m_Stats.ContainsKey(record.Key))
                {
                    m_Stats[record.Key].Value = record.Value;
                }
                else
                {
                    m_Stats.Add(record.Key, record);
                }
            }
            CacheTimerKeys();
        }

        void ISavable.RestoreState(object state)
        {
            if (state is JObject jObject)
            {
                RestoreSaveData(jObject.ToObject<StatsSaveData>());
            }
            else if (state is StatsSaveData data)
            {
                RestoreSaveData(data);
            }
        }

        public static void Increment(string key, float amount = 1)
        {
            if (Instance.m_Stats.TryGetValue(key, out var stat))
            {
                Instance.UpdateStat(key, stat.Value + amount);
            }
            else
            {
                Instance.RegisterStat(key, StatType.Counter);
                Instance.UpdateStat(key, Instance.m_Stats[key].Value + amount);
            }
        }

        public static void SetMax(string key, float value)
        {
            if (Instance.m_Stats.TryGetValue(key, out var stat))
            {
                if (value > stat.Value)
                {
                    Instance.UpdateStat(key, value);
                }
            }
            else
            {
                Instance.RegisterStat(key, StatType.Max);
                Instance.UpdateStat(key, value);
            }
        }

        public static float GetValue(string key)
        {
            return Instance.m_Stats.TryGetValue(key, out var stat) ? stat.Value : 0;
        }

        public void RegisterStat(string key, StatType type, string displayName = "")
        {
            if (!m_Stats.ContainsKey(key))
            {
                m_Stats.Add(key, new StatRecord(key, type, displayName));
                if (type == StatType.Timer)
                {
                    CacheTimerKeys();
                }
            }
        }

        private void UpdateStat(string key, float newValue, bool publishEvent = true)
        {
            if (m_Stats.TryGetValue(key, out var stat))
            {
                float oldValue = stat.Value;
                stat.Value = newValue;

                if (publishEvent && oldValue != newValue)
                {
                    MessageBroker.Global.Publish(
                        new StatChangedEvent(key, oldValue, newValue, stat.Type)
                    );
                }
            }
        }

        private void CacheTimerKeys()
        {
            m_CachedTimerKeys = m_Stats
                .Values.Where(s => s.Type == StatType.Timer)
                .Select(s => s.Key)
                .ToList();
        }

        public static StatsManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindFirstObjectByType<StatsManager>();
                    if (m_Instance == null)
                    {
                        GameObject go = new("StatsManager");
                        m_Instance = go.AddComponent<StatsManager>();
                    }
                }
                return m_Instance;
            }
        }

        public string SaveID => "GlobalStats";

        [Header("Default Stats")]
        [SerializeField]
        private List<StatRecord> m_InitialStats = new()
        {
            new StatRecord(StatKeys.GameDuration, StatType.Timer, "Game Duration"),
            new StatRecord(StatKeys.RespawnCount, StatType.Counter, "Respawns"),
            new StatRecord(StatKeys.PlayerJump, StatType.Counter, "Jumps"),
            new StatRecord(StatKeys.PlayerHook, StatType.Counter, "Hook Shots"),
            new StatRecord(StatKeys.PlayerWhistle, StatType.Counter, "Whistles"),
        };

        private static StatsManager m_Instance;
        private readonly Dictionary<string, StatRecord> m_Stats = new();
        private List<string> m_CachedTimerKeys = new();
    }
}
