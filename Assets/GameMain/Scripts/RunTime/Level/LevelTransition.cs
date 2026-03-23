using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    public class LevelTransition : MonoBehaviour
    {
        [ShowInInspector]
        public LevelTransition Target { get; set; }
    }
}
