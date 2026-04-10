using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Core
{
    [Serializable]
    public abstract class AudioEntry
    {
        public abstract void Execute(IEvent e, VgAudioSystem manager);

        protected static IEnumerable<ValueDropdownItem<Type>> GetEventTypes() =>
            AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => new ValueDropdownItem<Type>(t.Name, t));

        [OdinSerialize]
        [ValueDropdown(nameof(GetEventTypes))]
        public Type ListenEventType;

        public TriggerMode TriggerMode = TriggerMode.OnNext;
    }
}
