using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class TransformExtension
    {
        /// <summary>
        /// 在世界空间中移动物体, 不考虑父物体的旋转和缩放等.
        /// </summary>
        public static void MoveInWorldCoordDiscardHierachyTransformatino(
            this Transform transform,
            Vector3 worldOffset
        )
        {
            if (transform.parent == null)
            {
                transform.position += worldOffset;
            }
            else
            {
                var localOffset = transform.parent.InverseTransformVector(worldOffset);
                transform.localPosition += localOffset;
            }
        }
    }
}
