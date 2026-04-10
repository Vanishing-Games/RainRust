using System;
using FMODUnity;
using Sirenix.OdinInspector;

namespace Core
{
    [Serializable]
    public abstract class AudioPlayEntry : AudioEntry
    {
        public EventReference FmodEvent;

        [BoxGroup("Playback")]
        public AudioPlayMode PlayMode = AudioPlayMode.OneShot;

        [BoxGroup("Playback")]
        [ShowIf(nameof(PlayMode), AudioPlayMode.Managed)]
        public ManagedConfig Managed;
    }
}
