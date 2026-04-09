using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Core
{
    [Serializable]
    public class ManagedConfig
    {
        private static IEnumerable<ValueDropdownItem<Type>> GetEventTypes() =>
            AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => new ValueDropdownItem<Type>(t.Name, t));

        public string Id;

        [OdinSerialize]
        [ValueDropdown(nameof(GetEventTypes))]
        public Type StopEventType;

        public FMOD.Studio.STOP_MODE StopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

        public bool RestartIfPlaying;
    }
}
