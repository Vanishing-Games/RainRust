using System;
using FMODUnity;

namespace Core
{
    [Serializable]
    public class DirectPlay3DOneShotEntry : AudioEntry
    {
        public override void Execute(IEvent e, VgAudioManager manager)
        {
            if (e is not IFmodPositionEvent posEvent)
            {
                CLogger.LogWarn(
                    $"DirectPlay3DOneShotEntry: event {e?.GetType().Name} does not implement IFmodPositionEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            RuntimeManager.PlayOneShot(posEvent.FmodEvent, posEvent.Position);
        }
    }
}
