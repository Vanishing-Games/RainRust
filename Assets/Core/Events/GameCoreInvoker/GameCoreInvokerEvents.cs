using Core;
using UnityEngine;

namespace Core
{
    public class GameCoreInvokerEvents
    {
        public struct GameCorePreInitEvent : IEvent { }

        public struct GameCorePostInitEvent : IEvent { }

        public struct GameCoreLevelPreInitEvent : IEvent { }

        public struct GameCorePostEndEvent : IEvent { }

        public struct GameCoreQuickStartEvent : IEvent { }

        public abstract class GameCoreCustomInitEvent : IEvent { }
    }
}
