using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VanishingGames.ECC.Runtime;

namespace GameMain.RunTime
{
    public partial class PlayerControlComponent : EccComponent
    {
        [HideInInspector]
        public Subject<EccTag> TagChangeEvent = new Subject<EccTag>();
    }
}
