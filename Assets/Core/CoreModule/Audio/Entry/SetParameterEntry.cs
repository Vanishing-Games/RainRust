using System;
using Sirenix.OdinInspector;

namespace Core
{
    [Serializable]
    public class SetParameterEntry : AudioParameterEntry
    {
        [BoxGroup("Parameter")]
        public FmodParameterPair[] Parameters;

        protected override FmodParameterPair[] ResolveParameters(IEvent e) => Parameters;
    }
}
