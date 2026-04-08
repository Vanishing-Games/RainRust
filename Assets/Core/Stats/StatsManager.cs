using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using R3;
using UnityEngine;

namespace Core
{
    public class StatsManager : MonoSingletonPersistent<StatsManager>
    {
        protected override void Awake()
        {
            base.Awake();

            foreach (var stat in m_InitialStats)
            {
                if (!m_Stats.ContainsKey(stat.Key))
                {
                    m_Stats.Add(stat.Key, stat);
                }
            }

            CacheTimerKeys();
            SubscribeToSaveEvents();
        }

        private void Update()
        {
            foreach (var key in m_CachedTimerKeys)
            {
                UpdateStat(key, m_Stats[key].Value + Time.deltaTime, false);
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

        private void SubscribeToSaveEvents()
        {
            MessageBroker.Global.Subscribe<BeforeWriteSaveEvent>(OnBeforeWriteSave).AddTo(this);
            MessageBroker.Global.Subscribe<OnLoadSaveEvent>(OnLoadSave).AddTo(this);
        }

        private void OnBeforeWriteSave(BeforeWriteSaveEvent evt)
        {
            if (evt.IsGlobal)
            {
                return;
            }

            SaveManager.Instance.UpdateSaveValue(m_SaveID, CaptureSaveData());
        }

        private void OnLoadSave(OnLoadSaveEvent evt)
        {
            if (evt.IsGlobal)
            {
                return;
            }

            if (evt.Container.Data.TryGetValue(m_SaveID, out var state))
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
        }

        private StatsSaveData CaptureSaveData()
        {
            return new StatsSaveData { Stats = m_Stats.Values.ToList() };
        }

        private void RestoreSaveData(StatsSaveData data)
        {
            if (data?.Stats == null)
            {
                return;
            }

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

        private void UpdateStat(string key, float newValue, bool publishEvent = true)
        {
            if (m_Stats.TryGetValue(key, out var stat))
            {
                float oldValue = stat.Value;
                stat.Value = newValue;

                if (publishEvent && oldValue != newValue)
                {
                    MessageBroker.Global.Publish(new StatChangedEvent(key, oldValue, newValue, stat.Type));
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

        private readonly Dictionary<string, StatRecord> m_Stats = new();
        private List<string> m_CachedTimerKeys = new();
        private readonly string m_SaveID = "GlobalStats";
    }
}
