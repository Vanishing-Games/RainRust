using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace GameMain.RunTime
{

        public enum TailType
        {
            [LabelText("直线尾巴, 从下到上")]
            Line,

            [LabelText("直角尾巴, 从下到右")]
            Corner,
        }
    

}
