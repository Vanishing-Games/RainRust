using Core;
using FMODUnity;
using UnityEngine;

namespace Core
{
    public static class AudioManagerHelperEvents
    {
        public readonly struct PlayFmodOneShotEvent : IFmodPositionEvent
        {
            public PlayFmodOneShotEvent(EventReference fmodEvent, Vector3? position = null)
            {
                FmodEvent = fmodEvent;
                Position = position;
            }

            public EventReference FmodEvent { get; }
            public Vector3? Position { get; }

            Vector3 IPositionEvent.Position => Position ?? Vector3.zero;
        }

        public readonly struct PlayFmodManagedEvent : IEvent
        {
            public PlayFmodManagedEvent(EventReference fmodEvent) => FmodEvent = fmodEvent;

            public EventReference FmodEvent { get; }
        }

        // SetParameterFromEventEntry (ManagedId="bgm", ParameterName="intensity")
        public readonly struct BgmSetIntensityEvent : IFloatValueEvent
        {
            public BgmSetIntensityEvent(float value) => Value = value;

            public float Value { get; }
        }

        // Used as ManagedConfig.StopEventType in both BgmPlay entries
        public readonly struct StopFmodManagedEvent : IEvent { }

        // DirectSetParameterEntry — set any parameter on any managed instance
        public readonly struct SetManagedParameterEvent : IFmodParameterEvent
        {
            public SetManagedParameterEvent(string managedId, string parameterName, float value)
            {
                ManagedId = managedId;
                ParameterName = parameterName;
                Value = value;
            }

            public string ManagedId { get; }
            public string ParameterName { get; }
            public float Value { get; }
        }
    }
}
