using System;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class BeeEvents
    {
        public static Action<GameObject> OnBeeAwaken;

        public static void TriggerBeeAwaken(GameObject Bee)
        {
            OnBeeAwaken?.Invoke(Bee);
        }
    }
}
