using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 物理实体类型的基类 (如移动平台、障碍物)
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class LDtkSolidEntity : AutoLdtkEntity
    {
        protected virtual void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = false;
                col.size = BaseGridSize;
            }
        }
    }
}
