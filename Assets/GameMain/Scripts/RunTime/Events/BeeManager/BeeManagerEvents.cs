using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class BeeManagerEvents
    {
        public struct BeeAddEvents : IEvent
        {
            public GameObject Bee;

            public BeeAddEvents(GameObject bee)
            {
                Bee = bee;
            }
        }

        public struct BeeRemoveEvents : IEvent
        {
            public GameObject Bee;

            public BeeRemoveEvents(GameObject bee)
            {
                Bee = bee;
            }
        }
    }
}
