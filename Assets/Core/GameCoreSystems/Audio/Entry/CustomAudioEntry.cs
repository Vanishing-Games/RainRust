using System;
using FMODUnity;
using UnityEngine;

namespace Core
{
    [Serializable]
    public abstract class CustomAudioEntry : AudioPlayEntry
    {
        protected abstract FmodParameterPair[] ResolveParameters(IEvent e);

        protected virtual Vector3? ResolvePosition(IEvent e) => null;

        public override void Execute(IEvent e, VgAudioSystem manager)
        {
            CLogger.LogInfo(
                "CustomAudioEntry: Executing custom audio entry. Listened event: "
                    + ListenEventType.Name,
                LogTag.AudioEntry
            );

            var parameters = ResolveParameters(e);
            var position = ResolvePosition(e);

            if (PlayMode == AudioPlayMode.OneShot)
            {
                var instance = RuntimeManager.CreateInstance(FmodEvent);
                ApplyParameters(instance, parameters);

                if (position.HasValue)
                    instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

                instance.start();
                instance.release();
                return;
            }

            manager.PlayManagedCustom(FmodEvent, parameters, position, Managed);
        }

        private static void ApplyParameters(
            FMOD.Studio.EventInstance instance,
            FmodParameterPair[] parameters
        )
        {
            if (parameters == null)
                return;
            foreach (var p in parameters)
                instance.setParameterByName(p.Name, p.Value);
        }
    }
}
