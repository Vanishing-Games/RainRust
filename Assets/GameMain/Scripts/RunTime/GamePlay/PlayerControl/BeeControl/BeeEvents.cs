using System;
using System.Xml.Serialization;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class BeeEvents
    {
        public static Action<GameObject> OnBeeAwaken;
        public static Action<GameObject> OnBeeLeave;

        public static void TriggerBeeAwaken(GameObject Bee)
        {
            OnBeeAwaken?.Invoke(Bee);
        }

        public static void TriggerBeeLeave(GameObject Bee)
        {
            OnBeeLeave?.Invoke(Bee);
        }
    }
}
