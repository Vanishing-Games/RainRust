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
        private const int RetryMaxAttempts = 10;
        private const int RetryDelayMs = 200;

        public StringManagedAudioEntry() { }

        public override void Execute(IEvent e, VgAudioManager manager)
        {
            if (e is IStringValueEvent stringEvent)
            {
                CLogger.LogInfo(
                    "StringManagedAudioEntry: Executing string managed audio entry. Listened event: "
                        + ListenEventType.Name
                        + ", String value: "
                        + stringEvent.Value,
                    LogTag.AudioEntry
                );

                PlayWithRetryAsync(stringEvent.Value, manager).Forget();
            }
            else
            {
                CLogger.LogWarn(
                    "StringManagedAudioEntry: Received event is not of type IStringValueEvent. Listened event: "
                        + ListenEventType.Name
                        + ", Actual event type: "
                        + e.GetType().Name,
                    LogTag.AudioEntry
                );
            }
        }

        private async UniTaskVoid PlayWithRetryAsync(string value, VgAudioManager manager)
        {
            var eventName = "event:/Test/BGM/" + value;
            var managed = new ManagedConfig
            {
                Id = value,
                RestartIfPlaying = true,
                StopEventType = StopEventType,
            };

            for (var attempt = 0; attempt < RetryMaxAttempts; attempt++)
            {
                try
                {
                    var eventRef = EventReference.Find(eventName);
                    manager.PlayManaged(managed.Id, eventRef, managed);
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

        [OdinSerialize]
        [ValueDropdown(nameof(GetEventTypes))]
        public Type StopEventType;
    }
}
