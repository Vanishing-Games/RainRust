using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    public class VgAudioSystem : CoreModuleManagerBase<VgAudioSystem>, ICoreModuleSystem
    {
        public string SystemName => "VgAudioSystem";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnBootStart(async () =>
            {
                LoadAllBanks();
                RegisterAllEntries();
                await UniTask.CompletedTask;
            });

            registry.OnMainMenuEnter(async () => await PlayMenuBgm());
            registry.OnMainMenuExit(async () => await FadeOutBgm());
            registry.OnInLevelEnter(async ctx => await PlayLevelBgm(ctx.ChapterId));
            registry.OnInLevelExit(async () => await FadeOutBgm());
            registry.OnLoadStart(async _ => await FadeOutBgm());

            registry.OnGameQuit(async () =>
            {
                UnregisterAll();
                StopAllManaged();
                await UniTask.CompletedTask;
            });
        }

        public async UniTask PlayMenuBgm()
        {
            CLogger.LogInfo("Playing Menu BGM", LogTag.Audio);
            await UniTask.CompletedTask;
        }

        public async UniTask PlayLevelBgm(string chapterId)
        {
            CLogger.LogInfo($"Playing Level BGM for chapter: {chapterId}", LogTag.Audio);
            await UniTask.CompletedTask;
        }

        public async UniTask FadeOutBgm()
        {
            CLogger.LogInfo("Fading out BGM", LogTag.Audio);
            StopAllManaged();
            await UniTask.CompletedTask;
        }

        internal void PlayManaged(EventReference fmodEvent, ManagedConfig config)
        {
            if (m_ManagedInstances.ContainsKey(fmodEvent.Guid.ToString()))
            {
                if (!config.RestartIfPlaying)
                    return;
                StopManaged(fmodEvent.Guid.ToString(), config.StopMode);
            }

            var instance = RuntimeManager.CreateInstance(fmodEvent);
            instance.start();
            m_ManagedInstances[fmodEvent.Guid.ToString()] = instance;
            CLogger.LogInfo($"PlayManaged: {fmodEvent}", LogTag.Audio);
        }

        internal void PlayManaged3D(
            EventReference fmodEvent,
            Vector3 position,
            ManagedConfig config
        )
        {
            if (m_ManagedInstances.ContainsKey(fmodEvent.Guid.ToString()))
            {
                if (!config.RestartIfPlaying)
                    return;
                StopManaged(fmodEvent.Guid.ToString(), config.StopMode);
            }

            var instance = RuntimeManager.CreateInstance(fmodEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.start();
            m_ManagedInstances[fmodEvent.Guid.ToString()] = instance;
            CLogger.LogInfo($"PlayManaged3D: {fmodEvent}", LogTag.Audio);
        }

        internal void PlayManagedCustom(
            EventReference fmodEvent,
            FmodParameterPair[] parameters,
            Vector3? position,
            ManagedConfig config
        )
        {
            if (m_ManagedInstances.ContainsKey(fmodEvent.Guid.ToString()))
            {
                if (!config.RestartIfPlaying)
                    return;
                StopManaged(fmodEvent.Guid.ToString(), config.StopMode);
            }

            var instance = RuntimeManager.CreateInstance(fmodEvent);

            if (parameters != null)
                foreach (var p in parameters)
                    instance.setParameterByName(p.Name, p.Value);

            if (position.HasValue)
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

            instance.start();
            m_ManagedInstances[fmodEvent.Guid.ToString()] = instance;
            CLogger.LogInfo($"PlayManagedCustom: {fmodEvent}", LogTag.Audio);
        }

        internal void SetManagedParameter(string id, FmodParameterPair[] parameters)
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

        internal void StopManaged(string id, FMOD.Studio.STOP_MODE stopMode)
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

        private void LoadAllBanks()
        {
            try
            {
                if (!RuntimeManager.HaveAllBanksLoaded)
                {
                    CLogger.LogWarn(
                        "FMOD Banks not fully loaded yet, waiting for RuntimeManager...",
                        LogTag.Audio
                    );
                }
                else
                {
                    CLogger.LogInfo(
                        "All FMOD Banks confirmed loaded by RuntimeManager.",
                        LogTag.Audio
                    );
                }
            }
            catch (Exception e)
            {
                CLogger.LogError($"Failed to load FMOD Banks: {e.Message}", LogTag.Audio);
            }
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
                    if (entry == null)
                    {
                        CLogger.LogWarn(
                            $"Null entry found in sheet '{sheet.name}', skipping.",
                            LogTag.Audio
                        );
                        continue;
                    }

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
            var method = typeof(VgAudioSystem)
                .GetMethod(
                    nameof(SubscribeEntryGeneric),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .MakeGenericMethod(entry.ListenEventType);
            var disposable = (IDisposable)method.Invoke(this, new object[] { entry });
            m_Subscriptions.Add((entry.ListenEventType.Name, disposable));
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
            var method = typeof(VgAudioSystem)
                .GetMethod(
                    nameof(SubscribeStopGeneric),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .MakeGenericMethod(entry.Managed.StopEventType);
            var disposable = (IDisposable)
                method.Invoke(
                    this,
                    new object[] { entry.FmodEvent.Guid.ToString(), entry.Managed.StopMode }
                );
            m_Subscriptions.Add((entry.FmodEvent.Guid.ToString(), disposable));
        }

        internal void RegisterStopSubscription(
            string subscriptionId,
            Type eventType,
            FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT
        )
        {
            if (subscriptionId == null || eventType == null)
            {
                CLogger.LogWarn(
                    $"RegisterStopSubscription: Invalid parameters. subscriptionId: {subscriptionId}, eventType: {eventType}",
                    LogTag.Audio
                );
                return;
            }
            else if (m_Subscriptions.Exists(s => s.Item1 == subscriptionId))
            {
                CLogger.LogWarn(
                    $"RegisterStopSubscription: Subscription ID '{subscriptionId}' already exists. Skipping registration.",
                    LogTag.Audio
                );
                return;
            }

            var method = typeof(VgAudioSystem)
                .GetMethod(
                    nameof(SubscribeStopGeneric),
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
                .MakeGenericMethod(eventType);
            var disposable = (IDisposable)
                method.Invoke(this, new object[] { subscriptionId, stopMode });
            m_Subscriptions.Add((subscriptionId, disposable));
        }

        private IDisposable SubscribeStopGeneric<TEvent>(string id, FMOD.Studio.STOP_MODE stopMode)
            where TEvent : IEvent
        {
            return MessageBroker.Global.Subscribe<TEvent>(_ => StopManaged(id, stopMode));
        }

        private void UnregisterAll()
        {
            foreach (var sub in m_Subscriptions)
                sub.Item2.Dispose();
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
        private readonly List<(string, IDisposable)> m_Subscriptions = new();

        [BoxGroup("DEBUG")]
        [ReadOnly]
        [ShowInInspector]
        private readonly Dictionary<string, FMOD.Studio.EventInstance> m_ManagedInstances = new();
    }
}
