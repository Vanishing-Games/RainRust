using Core;
using FMODUnity;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class AudioManagerHelperEvents
    {
        // ─── BGM ─────────────────────────────────────────────────────────────────────
        // DefaultAudioEntry (Managed, Id="bgm", Stop=BgmStopEvent)
        public readonly struct BgmPlayDefaultEvent : IEvent { }

        // DirectPlayManagedEntry (Id="bgm", Stop=BgmStopEvent)
        public readonly struct BgmPlayEvent : IEvent
        {
            public BgmPlayEvent(EventReference fmodEvent) => FmodEvent = fmodEvent;

            public EventReference FmodEvent { get; }
        }

        // Used as ManagedConfig.StopEventType in both BgmPlay entries
        public readonly struct BgmStopEvent : IEvent { }

        // SetParameterFromEventEntry (ManagedId="bgm", ParameterName="intensity")
        public readonly struct BgmSetIntensityEvent : IFloatValueEvent
        {
            public BgmSetIntensityEvent(float value) => Value = value;

            public float Value { get; }
        }

        // ─── Ambience ────────────────────────────────────────────────────────────────
        // DefaultAudioEntry (Managed, Id="ambience", Stop=AmbienceStopEvent)
        public readonly struct AmbiencePlayDefaultEvent : IEvent { }

        // DirectPlayManagedEntry (Id="ambience", Stop=AmbienceStopEvent)
        public readonly struct AmbiencePlayEvent : IFmodOneShotEvent
        {
            public AmbiencePlayEvent(EventReference fmodEvent) => FmodEvent = fmodEvent;

            public EventReference FmodEvent { get; }
        }

        // Used as ManagedConfig.StopEventType in both AmbiencePlay entries
        public readonly struct AmbienceStopEvent : IEvent { }

        // SetParameterFromEventEntry (ManagedId="ambience", ParameterName="intensity")
        public readonly struct AmbienceSetIntensityEvent : IFloatValueEvent
        {
            public AmbienceSetIntensityEvent(float value) => Value = value;

            public float Value { get; }
        }

        // ─── UI Sounds ───────────────────────────────────────────────────────────────
        // DefaultAudioEntry (OneShot)
        public readonly struct UiConfirmSoundDefaultEvent : IEvent { }

        // DefaultAudioEntry (OneShot)
        public readonly struct UiCancelSoundDefaultEvent : IEvent { }

        // DefaultAudioEntry (OneShot)
        public readonly struct UiHoverSoundDefaultEvent : IEvent { }

        // DefaultAudioEntry (OneShot)
        public readonly struct UiErrorSoundDefaultEvent : IEvent { }

        // ─── Stinger ─────────────────────────────────────────────────────────────────
        // DefaultAudioEntry (OneShot)
        public readonly struct StingerPlayDefaultEvent : IEvent { }

        // DirectPlay3DOneShotEntry — position and FMOD event both from event
        public readonly struct SfxAtPositionEvent : IFmodPositionEvent
        {
            public SfxAtPositionEvent(EventReference fmodEvent, Vector3 position)
            {
                FmodEvent = fmodEvent;
                Position = position;
            }

            public EventReference FmodEvent { get; }
            public Vector3 Position { get; }
        }

        // ─── Generic Escape Hatch ────────────────────────────────────────────────────
        // DirectPlayOneShotEntry — play any FMOD event without sheet configuration
        public readonly struct PlayFmodOneShotEvent : IFmodOneShotEvent
        {
            public PlayFmodOneShotEvent(EventReference fmodEvent) => FmodEvent = fmodEvent;

            public EventReference FmodEvent { get; }
        }

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
