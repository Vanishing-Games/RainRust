using System;
using FMODUnity;

namespace Core
{
    [Serializable]
    public class DefaultAudioEntry : AudioPlayEntry
    {
        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            CLogger.LogInfo(
                $"DefaultAudioEntry: Received event {e?.GetType().Name}, executing with PlayMode {PlayMode}"
                    + "\nListenEventType: "
                    + ListenEventType?.Name
                    + "\nFmod Event: "
                    + FmodEvent,
                LogTag.AudioEntry
            );

            if (PlayMode == AudioPlayMode.OneShot)
            {
                RuntimeManager.PlayOneShot(FmodEvent);
                return;
            }

            manager.PlayManaged(FmodEvent, Managed);
        }
    }
}
