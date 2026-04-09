using System;
using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Core
{
    [Serializable]
    public class StringManagedAudioEntry : AudioEntry
    {
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

                var managed = new ManagedConfig
                {
                    Id = stringEvent.Value,
                    RestartIfPlaying = true,
                    StopEventType = StopEventType,
                };

                manager.PlayManaged(managed.Id, EventReference.Find(stringEvent.Value), managed);
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

        [OdinSerialize]
        [ValueDropdown(nameof(GetEventTypes))]
        public Type StopEventType;
    }
}
