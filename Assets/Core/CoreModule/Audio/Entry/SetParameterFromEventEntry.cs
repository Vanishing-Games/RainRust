using System;
using Sirenix.OdinInspector;

namespace Core
{
    [Serializable]
    public class SetParameterFromEventEntry : AudioParameterEntry
    {
        [BoxGroup("Parameter")]
        public string ParameterName;

        protected override FmodParameterPair[] ResolveParameters(IEvent e)
        {
            CLogger.LogInfo(
                $"SetParameterFromEventEntry: Resolving parameters from event {e?.GetType().Name} for ParameterName {ParameterName}",
                LogTag.AudioEntry
            );

            if (e is not IFloatValueEvent floatEvent)
            {
                CLogger.LogWarn(
                    $"SetParameterFromEventEntry: event {e?.GetType().Name} does not implement IFloatValueEvent",
                    LogTag.Audio
                );
                return null;
            }

            return new[]
            {
                new FmodParameterPair { Name = ParameterName, Value = floatEvent.Value },
            };
        }
    }
}
