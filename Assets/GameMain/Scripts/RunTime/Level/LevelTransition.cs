using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelTransition : MonoBehaviour
    {
        [SerializeField]
        public LevelTransition Target;

        internal Vector3 GetPlayerFeetSpawnPoint()
        {
            return transform.position;
        }
    }
}
