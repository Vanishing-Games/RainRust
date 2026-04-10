using System;
using Sirenix.OdinInspector;

namespace Core
{
    [Serializable]
    public abstract class AudioParameterEntry : AudioEntry
    {
        protected abstract FmodParameterPair[] ResolveParameters(IEvent e);

        protected virtual string GetTargetId(IEvent e) => ManagedId;

        public override void Execute(IEvent e, VgAudioManager manager)
        {
            CLogger.LogInfo(
                $"AudioParameterEntry: Received event {e?.GetType().Name}, executing with ManagedId {ManagedId}, TriggerMode {TriggerMode}",
                LogTag.AudioEntry
            );

            var parameters = ResolveParameters(e);
            if (parameters == null || parameters.Length == 0)
                return;
            manager.SetManagedParameter(GetTargetId(e), parameters);
        }

        [BoxGroup("Parameter")]
        public string ManagedId;
    }
}
