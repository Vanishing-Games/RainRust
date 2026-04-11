using System;
using FMODUnity;

namespace Core
{
    [Serializable]
    public class DirectPlay3DOneShotEntry : AudioEntry
    {
        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            if (e is not IFmodPositionEvent posEvent)
            {
                CLogger.LogWarn(
                    $"DirectPlay3DOneShotEntry: event {e?.GetType().Name} does not implement IFmodPositionEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            CLogger.LogInfo($"DirectPlay3DOneShotEntry: Playing 3D OneShot — ListenEvent={ListenEventType?.Name}, FmodEvent={posEvent.FmodEvent}, Pos={posEvent.Position}", LogTag.AudioEntry);
            RuntimeManager.PlayOneShot(posEvent.FmodEvent, posEvent.Position);
        }
    }
}
