using System;
using FMODUnity;

namespace Core
{
    [Serializable]
    public class DirectPlayOneShotEntry : AudioEntry
    {
        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            if (e is not IFmodOneShotEvent oneShotEvent)
            {
                CLogger.LogWarn(
                    $"DirectPlayOneShotEntry: event {e?.GetType().Name} does not implement IFmodOneShotEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            CLogger.LogInfo($"DirectPlayOneShotEntry: Playing OneShot — ListenEvent={ListenEventType?.Name}, FmodEvent={oneShotEvent.FmodEvent}", LogTag.AudioEntry);
            RuntimeManager.PlayOneShot(oneShotEvent.FmodEvent);
        }
    }
}
