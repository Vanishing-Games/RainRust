using UnityEngine;

namespace PlayerControlByOris
{
    public class WaveLife : MonoBehaviour
    {
        [Header("扩散配置")]
        public float maxRadius = 5f; // 预设的最大半径
        public float duration = 1.5f; // 整个扩张过程持续时间

        [Header("动画曲线 (最核心部分)")]
        // 默认设为 EaseOut (开始快，后面慢)
        public AnimationCurve expansionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // 控制透明度的曲线 (从不透明变为全透明)
        public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private SpriteRenderer spriteRenderer;
        private float timer = 0f;
        private Vector3 initialScale;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialScale = transform.localScale;

            // 初始透明度
            SetAlpha(fadeCurve.Evaluate(0f));
        }

        void Update()
        {
            timer += Time.deltaTime;

            // 计算当前动画进度 (0f 到 1f)
            float progress = Mathf.Clamp01(timer / duration);

            // 1. 根据曲线计算并设置缩放 (关键：非匀速扩大的秘密)
            float curveValue = expansionCurve.Evaluate(progress);
            float currentScale = curveValue * maxRadius;
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // 2. 根据曲线计算并设置透明度
            float currentAlpha = fadeCurve.Evaluate(progress);
            SetAlpha(currentAlpha);

            // 3. 动画结束时自动销毁
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
    }
}
