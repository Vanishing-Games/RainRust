using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 触发器类型的实体基类 (如存档点、收集品)
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class LDtkTriggerEntity : AutoLdtkEntity
    {
        protected virtual void Awake()
        {
            if (TryGetComponent<BoxCollider2D>(out var col))
            {
                col.isTrigger = true;
                col.size = BaseGridSize;
            }
        }

        protected abstract void OnTriggerEnter2D(Collider2D other);
    }
}
