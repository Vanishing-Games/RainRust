using System;

namespace Core
{
    [Serializable]
    public class DirectSetParameterEntry : AudioEntry
    {
        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            if (e is not IFmodParameterEvent paramEvent)
            {
                CLogger.LogWarn(
                    $"DirectSetParameterEntry: event {e?.GetType().Name} does not implement IFmodParameterEvent",
                    LogTag.AudioEntry
                );
                return;
            }

            CLogger.LogInfo($"DirectSetParameterEntry: Setting param — ListenEvent={ListenEventType?.Name}, ManagedId={paramEvent.ManagedId}, Param={paramEvent.ParameterName}={paramEvent.Value}", LogTag.AudioEntry);
            manager.SetManagedParameter(
                paramEvent.ManagedId,
                new[]
                {
                    new FmodParameterPair
                    {
                        Name = paramEvent.ParameterName,
                        Value = paramEvent.Value,
                    },
                }
            );
        }
    }
}
