using System;
using FMODUnity;

namespace Core
{
    [Serializable]
    public class DirectPlayOneShotEntry : AudioEntry
    {
        public override void Execute(IEvent e, VgAudioManager manager)
        {
            if (e is not IFmodOneShotEvent oneShotEvent)
            {
                CLogger.LogWarn(
                    $"DirectPlayOneShotEntry: event {e?.GetType().Name} does not implement IFmodOneShotEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            RuntimeManager.PlayOneShot(oneShotEvent.FmodEvent);
        }
    }
}
