using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    public class VgAudioManager : MonoSingletonPersistent<VgAudioManager>
    {
        public void PlayManaged(string id, EventReference fmodEvent, ManagedConfig config)
        {
            if (m_ManagedInstances.ContainsKey(id))
            {
                if (!config.RestartIfPlaying)
                    return;
                StopManaged(id, config.StopMode);
            }

            var instance = RuntimeManager.CreateInstance(fmodEvent);
            instance.start();
            m_ManagedInstances[id] = instance;
            CLogger.LogInfo($"PlayManaged: {id}", LogTag.Audio);
        }

        public void PlayManaged3D(
            string id,
            EventReference fmodEvent,
            Vector3 position,
            ManagedConfig config
        )
        {
            if (m_ManagedInstances.ContainsKey(id))
            {
                if (!config.RestartIfPlaying)
                    return;
                StopManaged(id, config.StopMode);
            }

            var instance = RuntimeManager.CreateInstance(fmodEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.start();
            m_ManagedInstances[id] = instance;
            CLogger.LogInfo($"PlayManaged3D: {id}", LogTag.Audio);
        }

        public void PlayManagedCustom(
            string id,
            EventReference fmodEvent,
            FmodParameterPair[] parameters,
            Vector3? position,
            ManagedConfig config
        )
        {
            if (m_ManagedInstances.ContainsKey(id))
            {
                if (!config.RestartIfPlaying)
                    return;
                StopManaged(id, config.StopMode);
            }

            var instance = RuntimeManager.CreateInstance(fmodEvent);

            if (parameters != null)
                foreach (var p in parameters)
                    instance.setParameterByName(p.Name, p.Value);

            if (position.HasValue)
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

            instance.start();
            m_ManagedInstances[id] = instance;
            CLogger.LogInfo($"PlayManagedCustom: {id}", LogTag.Audio);
        }

        public void SetManagedParameter(string id, FmodParameterPair[] parameters)
        {
            if (!m_ManagedInstances.TryGetValue(id, out var instance))
            {
                CLogger.LogWarn(
                    $"SetManagedParameter: Managed instance '{id}' not found",
                    LogTag.Audio
                );
                return;
            }

            foreach (var p in parameters)
                instance.setParameterByName(p.Name, p.Value);
        }

        public void StopManaged(string id, FMOD.Studio.STOP_MODE stopMode)
        {
            if (!m_ManagedInstances.TryGetValue(id, out var instance))
            {
                CLogger.LogWarn($"StopManaged: Managed instance '{id}' not found", LogTag.Audio);
                return;
            }

            instance.stop(stopMode);
            instance.release();
            m_ManagedInstances.Remove(id);
            CLogger.LogInfo($"StopManaged: {id}", LogTag.Audio);
        }

        public bool IsManagedPlaying(string id)
        {
            if (!m_ManagedInstances.TryGetValue(id, out var instance))
                return false;

            instance.getPlaybackState(out var state);
            return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
        }

        protected override void Awake()
        {
            base.Awake();
            LoadAllBanks();
            RegisterAllEntries();
        }

        private void OnDestroy()
        {
            UnregisterAll();
            StopAllManaged();
        }

        private void LoadAllBanks()
        {
            CLogger.LogInfo("All FMOD Banks loaded", LogTag.Audio);
        }

        private void RegisterAllEntries()
        {
            CLogger.LogVerbose("=== Starting Audio Event Registration ===", LogTag.Audio);

            var sb = new StringBuilder();
            sb.AppendLine("\n[Audio Registration Report]");

            foreach (var sheet in m_Sheets)
            {
                sb.AppendLine($"[SheetName] : {sheet?.name ?? "null"}");

                if (sheet == null)
                    continue;

                foreach (var entry in sheet.Entries)
                {
                    string entryType = entry?.GetType().Name ?? "null";
                    string listenEvent = entry?.ListenEventType?.Name ?? "null";
                    string triggerMode = entry?.TriggerMode.ToString() ?? "null";

                    sb.AppendLine(
                        $"    |-- [EntryType] : {entryType, -20} | [ListenEvent] : {listenEvent, -20} | [TriggerMode] : {triggerMode}"
                    );

                    if (entry?.ListenEventType == null)
                        continue;

                    SubscribeEntry(entry);

                    if (
                        entry is AudioPlayEntry playEntry
                        && playEntry.PlayMode == AudioPlayMode.Managed
                        && playEntry.Managed?.StopEventType != null
                        && playEntry.Managed.StopEventType != playEntry.ListenEventType
                    )
                    {
                        string stopEventName = playEntry.Managed.StopEventType.Name;
                        sb.AppendLine($"    |    └-- [StopEvent] : {stopEventName} (Managed Mode)");
                        RegisterStopSubscription(playEntry);
                    }
                }
                sb.AppendLine();
            }

            CLogger.LogInfo(sb.ToString(), LogTag.Audio);
        }

        private void SubscribeEntry(AudioEntry entry)
        {
            var method = typeof(VgAudioManager)
                .GetMethod(
                    nameof(SubscribeEntryGeneric),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .MakeGenericMethod(entry.ListenEventType);
            var disposable = (IDisposable)method.Invoke(this, new object[] { entry });
            m_Subscriptions.Add(disposable);
        }

        private IDisposable SubscribeEntryGeneric<TEvent>(AudioEntry entry)
            where TEvent : IEvent
        {
            return MessageBroker.Global.Subscribe<TEvent>(
                onNext: e =>
                {
                    if (entry.TriggerMode == TriggerMode.OnNext)
                        entry.Execute(e, this);
                },
                onError: ex =>
                {
                    if (entry.TriggerMode == TriggerMode.OnError)
                        entry.Execute(null, this);
                },
                onCompleted: () =>
                {
                    if (entry.TriggerMode == TriggerMode.OnComplete)
                        entry.Execute(null, this);
                }
            );
        }

        private void RegisterStopSubscription(AudioPlayEntry entry)
        {
            var method = typeof(VgAudioManager)
                .GetMethod(
                    nameof(SubscribeStopGeneric),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .MakeGenericMethod(entry.Managed.StopEventType);
            var disposable = (IDisposable)
                method.Invoke(this, new object[] { entry.Managed.Id, entry.Managed.StopMode });
            m_Subscriptions.Add(disposable);
        }

        private IDisposable SubscribeStopGeneric<TEvent>(string id, FMOD.Studio.STOP_MODE stopMode)
            where TEvent : IEvent
        {
            return MessageBroker.Global.Subscribe<TEvent>(_ => StopManaged(id, stopMode));
        }

        private void UnregisterAll()
        {
            foreach (var sub in m_Subscriptions)
                sub.Dispose();
            m_Subscriptions.Clear();
        }

        private void StopAllManaged()
        {
            foreach (var kvp in m_ManagedInstances)
            {
                kvp.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                kvp.Value.release();
            }
            m_ManagedInstances.Clear();
        }

        [SerializeField]
        [ListDrawerSettings(ShowFoldout = true)]
        private AudioEventSheet[] m_Sheets;

        [BoxGroup("DEBUG")]
        [ReadOnly]
        [ShowInInspector]
        private readonly List<IDisposable> m_Subscriptions = new();

        [BoxGroup("DEBUG")]
        [ReadOnly]
        [ShowInInspector]
        private readonly Dictionary<string, FMOD.Studio.EventInstance> m_ManagedInstances = new();
    }
}
