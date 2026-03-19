using Sirenix.OdinInspector;
using UnityEngine;
using CameraMode = GameMain.LDtk.CameraMode;

namespace GameMain.RunTime
{
    public class LevelRoom : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        public CameraMode CameraMode { get; set; }

        [ShowInInspector, ReadOnly]
        public Bounds BorderBounds { get; set; }
    }
}
