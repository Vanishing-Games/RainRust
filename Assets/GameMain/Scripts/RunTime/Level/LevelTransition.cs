using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelTransition : MonoBehaviour
    {
        [ShowInInspector]
        public LevelTransition Target { get; set; }

        internal Vector3 GetPlayerSpawnPoint()
        {
            return transform.position;
        }
    }
}
