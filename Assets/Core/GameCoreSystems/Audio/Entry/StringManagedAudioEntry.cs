using System;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Core
{
    [Serializable]
    public class StringManagedAudioEntry : AudioEntry
    {
        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            if (e is IStringValueEvent stringEvent)
            {
                CLogger.LogInfo(
                    "StringManagedAudioEntry: Executing. ListenEvent: "
                        + ListenEventType.Name
                        + ", Value: "
                        + stringEvent.Value,
                    LogTag.AudioEntry
                );

                PlayWithRetryAsync(stringEvent.Value, manager).Forget();
            }
            else
            {
                CLogger.LogWarn(
                    "StringManagedAudioEntry: Event is not IStringValueEvent. ListenEvent: "
                        + ListenEventType.Name
                        + ", Actual: "
                        + e.GetType().Name,
                    LogTag.AudioEntry
                );
            }
        }

        private async UniTaskVoid PlayWithRetryAsync(string value, VgAudioSystem manager)
        {
            var eventName = EventPathPrefix + value;
            var managed = new ManagedConfig
            {
                RestartIfPlaying = true,
                StopEventType = StopEventType,
            };

            for (var attempt = 0; attempt < RetryMaxAttempts; attempt++)
            {
                try
                {
                    var eventRef = EventReference.Find(eventName);
                    manager.PlayManaged(eventRef, managed);
                    manager.RegisterStopSubscription(eventName, StopEventType);
                    return;
                }
                catch (InvalidOperationException)
                {
                    CLogger.LogWarn(
                        $"StringManagedAudioEntry: EventManager not ready, retrying ({attempt + 1}/{RetryMaxAttempts})...",
                        LogTag.AudioEntry
                    );
                    await UniTask.Delay(RetryDelayMs);
                }
            }

            CLogger.LogError(
                $"StringManagedAudioEntry: Failed to find event '{eventName}' after {RetryMaxAttempts} attempts.",
                LogTag.AudioEntry
            );
        }

        private const int RetryMaxAttempts = 10;
        private const int RetryDelayMs = 200;

        [InfoBox("EventPath prefix appended before the string value from the event.")]
        public string EventPathPrefix = "event:/Test/BGM/";

        [OdinSerialize]
        [ValueDropdown(nameof(GetEventTypes))]
        public Type StopEventType;
    }
}
