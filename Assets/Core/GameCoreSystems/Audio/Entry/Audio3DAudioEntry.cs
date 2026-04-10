using System;
using FMODUnity;

namespace Core
{
    [Serializable]
    public class Audio3DAudioEntry : AudioPlayEntry
    {
        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            CLogger.LogInfo(
                $"Audio3DAudioEntry: Received event {e?.GetType().Name}, executing with PlayMode {PlayMode}",
                LogTag.AudioEntry
            );

            if (e is not IPositionEvent posEvent)
            {
                CLogger.LogWarn(
                    $"Audio3DAudioEntry: event {e?.GetType().Name} does not implement IPositionEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            if (PlayMode == AudioPlayMode.OneShot)
            {
                RuntimeManager.PlayOneShot(FmodEvent, posEvent.Position);
                return;
            }

            manager.PlayManaged3D(FmodEvent, posEvent.Position, Managed);
        }
    }
}
