using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 存档点实体：演示新管线的使用
    /// </summary>
    public class SavePoint : LDtkTriggerEntity
    {
        [LDtkField]
        public string PointName;

        [LDtkField]
        public string NickName;

        [Header("Visuals")]
        [SerializeField]
        protected SpriteRenderer m_SpriteRenderer;

        [SerializeField]
        protected Sprite m_UnsavedSprite;

        [SerializeField]
        protected Sprite m_SavedSprite;

        [SerializeField]
        protected Animator m_Animator;

        [SerializeField]
        protected string m_SaveAnimationTrigger = "Save";

        private bool m_IsSaved;

        protected override void Awake()
        {
            base.Awake();
            if (m_SpriteRenderer != null && m_UnsavedSprite != null)
            {
                m_SpriteRenderer.sprite = m_UnsavedSprite;
            }
        }

        public override void OnPostImport()
        {
            // 如果 LDtk 没有配置名字，则自动生成一个唯一的关卡内名字
            if (string.IsNullOrEmpty(PointName))
            {
                string worldId = World != null ? World.Identifier : "World";
                string levelId = Level != null ? Level.Identifier : "Level";
                PointName = $"{worldId}_{levelId}_{LdtkIid}";
            }

            // 确保在导入阶段也同步一下初始视觉
            if (m_SpriteRenderer != null && m_UnsavedSprite != null)
            {
                m_SpriteRenderer.sprite = m_UnsavedSprite;
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (m_IsSaved)
                return;

            if (other.CompareTag("Player"))
            {
                m_IsSaved = true;
                PlaySaveVisuals();
                OnSaveTriggered();
            }
        }

        private void OnSaveTriggered()
        {
            CLogger.LogInfo($"Player reached save point: {PointName} ({NickName})", LogTag.Game);
            // 假设 VgSaveSystem 已存在
            VgSaveSystem.Instance.WriteSlotSaveAsync().Forget();
        }

        private void PlaySaveVisuals()
        {
            if (m_Animator != null)
            {
                m_Animator.SetTrigger(m_SaveAnimationTrigger);
            }
            else if (m_SpriteRenderer != null && m_SavedSprite != null)
            {
                m_SpriteRenderer.sprite = m_SavedSprite;
            }
        }
    }
}
