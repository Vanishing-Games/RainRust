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
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                // Collider 尺寸应对应 Prefab 的基准格数大小
                col.size = BaseGridSize;
            }
        }

        protected abstract void OnTriggerEnter2D(Collider2D other);
    }
}
