using System;
using Sirenix.OdinInspector;

namespace Core
{
    [Serializable]
    public class DirectPlayManagedEntry : AudioEntry
    {
        [BoxGroup("Playback")]
        public ManagedConfig Managed;

        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            if (e is not IFmodOneShotEvent oneShotEvent)
            {
                CLogger.LogWarn(
                    $"DirectPlayManagedEntry: event {e?.GetType().Name} does not implement IFmodOneShotEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            CLogger.LogInfo(
                $"DirectPlayManagedEntry: Playing Managed — ListenEvent={ListenEventType?.Name}, FmodEvent={oneShotEvent.FmodEvent}",
                LogTag.AudioEntry
            );
            manager.PlayManaged(oneShotEvent.FmodEvent, Managed);
        }
    }
}
