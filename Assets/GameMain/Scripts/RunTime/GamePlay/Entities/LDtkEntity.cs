using LDtkUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 所有 LDtk 自动实体的基类
    /// </summary>
    public abstract class AutoLdtkEntity : MonoBehaviour
    {
        [ReadOnly, BoxGroup("LDtk Meta")]
        public string LdtkIid;

        [ReadOnly, BoxGroup("LDtk Meta")]
        public Vector2 LdtkGridSize;

        [ReadOnly, BoxGroup("LDtk Meta")]
        public LDtkComponentLevel Level;

        [ReadOnly, BoxGroup("LDtk Meta")]
        public LDtkComponentWorld World;

        [Header("Sizing Policy")]
        [Tooltip("该实体在 Unity 里的基准尺寸 (单位:格/米)。例如 1x1")]
        public Vector2 BaseGridSize = Vector2.one;

        public bool CanResize = false;

        [ShowIf("CanResize")]
        public bool KeepAspect = true;

        [ShowIf("CanResize")]
        public Vector2 MinGridSize = Vector2.zero;

        [ShowIf("CanResize")]
        public Vector2 MaxGridSize = new(999, 999);

        public virtual bool OnSyncFromLdtk(LDtkComponentEntity ldtkEntity)
        {
            var ldtkGridSize = ldtkEntity.Size;
            this.LdtkGridSize = ldtkGridSize;

            if (!CanResize)
            {
                if (
                    !Mathf.Approximately(ldtkGridSize.x, BaseGridSize.x)
                    || !Mathf.Approximately(ldtkGridSize.y, BaseGridSize.y)
                )
                {
                    Core.CLogger.LogError(
                        $"[LDtkEntity] {name} (IID:{LdtkIid}) 尺寸错误! LDtk 大小为 {ldtkGridSize}格，但预制体设置为不可缩放且基准大小为 {BaseGridSize}格。",
                        Core.LogTag.LDtkAutoEntityProcessor
                    );
                    return false;
                }
            }
            else
            {
                if (KeepAspect)
                {
                    float ldtkAspect = ldtkGridSize.x / ldtkGridSize.y;
                    float baseAspect = BaseGridSize.x / BaseGridSize.y;
                    if (!Mathf.Approximately(ldtkAspect, baseAspect))
                    {
                        Core.CLogger.LogError(
                            $"[LDtkEntity] {name} (IID:{LdtkIid}) 比例错误! LDtk 比例为 {ldtkAspect}，但预制体要求固定比例 {baseAspect}。",
                            Core.LogTag.LDtkAutoEntityProcessor
                        );
                        return false;
                    }
                }

                if (
                    ldtkGridSize.x < MinGridSize.x
                    || ldtkGridSize.y < MinGridSize.y
                    || ldtkGridSize.x > MaxGridSize.x
                    || ldtkGridSize.y > MaxGridSize.y
                )
                {
                    Core.CLogger.LogError(
                        $"[LDtkEntity] {name} (IID:{LdtkIid}) 超出尺寸约束! LDtk 大小为 {ldtkGridSize}格，范围要求 [{MinGridSize}, {MaxGridSize}]。",
                        Core.LogTag.LDtkAutoEntityProcessor
                    );
                    return false;
                }
            }

            if (!CanResize)
                return true;

            transform.localScale = new Vector3(
                ldtkGridSize.x / BaseGridSize.x,
                ldtkGridSize.y / BaseGridSize.y,
                1
            );
            var upLeftWorldPos = ldtkEntity.transform.position;
            var scale = ldtkGridSize / BaseGridSize;
            var finalPos = upLeftWorldPos + new Vector3(scale.x * 0.5f, scale.y * -0.5f, 0);
            transform.position = finalPos;

            return true;
        }

        public virtual void OnPostImport() { }
    }
}
