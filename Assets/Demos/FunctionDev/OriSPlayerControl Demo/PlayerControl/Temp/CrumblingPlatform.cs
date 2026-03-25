using System.Collections;
using UnityEngine;

namespace PlayerControlByOris
{
    public class CrumblingPlatform : MonoBehaviour
    {
        [Header("计时设置")]
        public float fallDelay = 1.5f; // 站上去多久后碎裂
        public float respawnDelay = 3.0f; // 碎裂后多久重新出现

        [Header("视觉反馈")]
        public Color warningColor = Color.red; // 快碎时的颜色

        private Vector3 initialPosition;
        private Color initialColor;
        private Collider2D platformCollider;
        private SpriteRenderer spriteRenderer;
        private bool isCrumbling = false;

        void Start()
        {
            initialPosition = transform.position;
            platformCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialColor = spriteRenderer.color;
        }

        // 检测玩家站上来
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 检查碰撞物体是否是玩家 (建议给玩家设置 Tag 为 "Player")
            if (collision.gameObject.CompareTag("Player") && !isCrumbling)
            {
                // 检查玩家是否是从上方踩下来的 (防止侧碰也触发)
                if (collision.contacts[0].normal.y < -0.5f)
                {
                    StartCoroutine(CrumbleSequence());
                }
            }
        }

        IEnumerator CrumbleSequence()
        {
            isCrumbling = true;

            // --- 第一阶段：预警 (抖动或变色) ---
            float elapsed = 0;
            while (elapsed < fallDelay)
            {
                elapsed += Time.deltaTime;
                // 简单的变色反馈
                spriteRenderer.color = Color.Lerp(initialColor, warningColor, elapsed / fallDelay);

                // 稍微加一点随机抖动感
                //transform.position = initialPosition + (Vector3)Random.insideUnitCircle * 0.05f;
                yield return null;
            }

            // --- 第二阶段：消失 (碎裂) ---
            platformCollider.enabled = false;
            spriteRenderer.enabled = false;
            transform.position = initialPosition; // 重置位置

            // --- 第三阶段：等待刷新 ---
            yield return new WaitForSeconds(respawnDelay);

            // --- 第四阶段：重置 ---
            spriteRenderer.color = initialColor;
            spriteRenderer.enabled = true;
            platformCollider.enabled = true;
            isCrumbling = false;
        }
    }
}
