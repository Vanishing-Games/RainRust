using Core;
using FMODUnity;
using UnityEngine;

namespace Core
{
    public static class AudioManagerHelperCommands
    {
        public class PlayFmodOneShotCommand : ICommand<bool>
        {
            public PlayFmodOneShotCommand(string fmodEventPath)
            {
                FmodEvent = RuntimeManager.PathToEventReference(fmodEventPath);
            }

            public EventReference FmodEvent { get; }

            public bool Execute()
            {
                RuntimeManager.PlayOneShot(FmodEvent);
                return true;
            }
        }

        public class PlayBgmCommand : ICommand<bool>
        {
            public PlayBgmCommand(string fmodEventPath)
            {
                FmodEvent = RuntimeManager.PathToEventReference(fmodEventPath);
            }

            public EventReference FmodEvent { get; }

            public bool Execute()
            {
                var bgmPlayEvent = new AudioManagerHelperEvents.PlayFmodManagedEvent(FmodEvent);
                MessageBroker.Global.Publish(bgmPlayEvent);
                return true;
            }
        }

        public class StopBgmCommand : ITriggerCommand
        {
            public StopBgmCommand() { }

            public bool Execute()
            {
                var bgmStopEvent = new AudioManagerHelperEvents.StopFmodManagedEvent();
                MessageBroker.Global.Publish(bgmStopEvent);
                return true;
            }
        }

        public class SetBgmIntensityCommand : ICommand<bool>
        {
            public SetBgmIntensityCommand(float intensity)
            {
                Intensity = intensity;
            }

            public float Intensity { get; }

            public bool Execute()
            {
                var setIntensityEvent = new AudioManagerHelperEvents.SetManagedParameterEvent(
                    managedId: "bgm",
                    parameterName: "intensity",
                    value: Intensity
                );
                MessageBroker.Global.Publish(setIntensityEvent);
                return true;
            }
        }
    }
}
