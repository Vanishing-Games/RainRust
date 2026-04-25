using System.Numerics;
using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class GamePlaySnakeGameEvents
    {
        public struct SnakeDeathEvent : IEvent { }

        public struct SnakeSaveEvent : IEvent { }

        public struct SnakeCheckPointEvent : IEvent
        {
            public UnityEngine.Vector3 Postion;

            public SnakeCheckPointEvent(UnityEngine.Vector3 postion)
            {
                Postion = postion;
            }
        }

        public struct HoneyCollectedEvent : IEvent
        {
            public SnakeHoney Honey;
        }

        public struct HoneyResetEvent : IEvent
        {
            public SnakeHoney Honey;
        }
    }
}
