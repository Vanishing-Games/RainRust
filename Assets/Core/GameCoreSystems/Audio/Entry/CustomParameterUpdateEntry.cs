using System;

namespace Core
{
    [Serializable]
    public abstract class CustomParameterUpdateEntry : AudioParameterEntry
    {
        protected virtual string ResolveManagedId(IEvent e) => ManagedId;

        protected override string GetTargetId(IEvent e) => ResolveManagedId(e);
    }
}
