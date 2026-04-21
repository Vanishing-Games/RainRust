using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class VgLoadingTransition : MonoProgressable
    {
        public static event Action OnAllTransitionsHidden;
        public static event Action OnAllTransitionsShown;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            DOTween.Init();
        }

        public override void Show()
        {
            CLogger.LogInfo($"Show called on {gameObject.name}", LogTag.Loading);
            if (m_ActiveShowTransitions == 0 && m_CompletedShowTransitions == 0)
                ResetCounters();

            m_AnimationQueue.Enqueue(() => ExecuteShow());
            ProcessQueue();
        }

        public override void Hide()
        {
            if (m_ActiveHideTransitions == 0 && m_CompletedHideTransitions == 0)
                ResetCounters();

            m_AnimationQueue.Enqueue(() => ExecuteHide());
            ProcessQueue();
        }

        private static void ResetCounters()
        {
            m_ActiveHideTransitions = 0;
            m_CompletedHideTransitions = 0;
            m_ActiveShowTransitions = 0;
            m_CompletedShowTransitions = 0;
        }

        private void ProcessQueue()
        {
            if (m_IsProcessingQueue || m_AnimationQueue.Count == 0)
                return;

            m_IsProcessingQueue = true;
            var nextAction = m_AnimationQueue.Dequeue();
            nextAction?.Invoke();
        }

        private void ExecuteShow()
        {
            transform.localScale = m_HideScale;
            if (m_CanvasGroup != null)
                m_CanvasGroup.alpha = 0f;

            if (m_CurrentTween != null && m_CurrentTween.IsActive())
                m_CurrentTween.Kill();

            gameObject.SetActive(true);
            m_ActiveShowTransitions++;

            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOScale(m_ShowScale, m_AnimationDuration).SetEase(m_EaseType));
            if (m_CanvasGroup != null && m_UseFade)
                seq.Join(
                    DOTween
                        .To(
                            () => m_CanvasGroup.alpha,
                            x => m_CanvasGroup.alpha = x,
                            1f,
                            m_AnimationDuration
                        )
                        .SetEase(m_EaseType)
                );

            bool completed = false;
            m_CurrentTween = seq.OnComplete(() =>
            {
                if (completed)
                    return;
                completed = true;
                FinishShow();
            });

            UniTask
                .Delay(TimeSpan.FromSeconds(m_AnimationDuration + 0.1f))
                .ContinueWith(() =>
                {
                    if (!completed)
                    {
                        completed = true;
                        if (m_CurrentTween != null && m_CurrentTween.IsActive())
                            m_CurrentTween.Kill();
                        FinishShow();
                    }
                })
                .Forget();
        }

        private void FinishShow()
        {
            m_CurrentTween = null;
            m_IsProcessingQueue = false;
            m_CompletedShowTransitions++;

            if (m_CompletedShowTransitions >= m_ActiveShowTransitions)
            {
                OnAllTransitionsShown?.Invoke();
                m_CompletedShowTransitions = 0;
                m_ActiveShowTransitions = 0;
            }

            ProcessQueue();
        }

        private void ExecuteHide()
        {
            if (m_CurrentTween != null && m_CurrentTween.IsActive())
                m_CurrentTween.Kill();

            m_ActiveHideTransitions++;

            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOScale(m_HideScale, m_AnimationDuration).SetEase(m_EaseType));
            if (m_CanvasGroup != null && m_UseFade)
                seq.Join(
                    DOTween
                        .To(
                            () => m_CanvasGroup.alpha,
                            x => m_CanvasGroup.alpha = x,
                            0f,
                            m_AnimationDuration
                        )
                        .SetEase(m_EaseType)
                );

            bool completed = false;
            m_CurrentTween = seq.OnComplete(() =>
            {
                if (completed)
                    return;
                completed = true;
                FinishHide();
            });

            UniTask
                .Delay(TimeSpan.FromSeconds(m_AnimationDuration + 0.1f))
                .ContinueWith(() =>
                {
                    if (!completed)
                    {
                        completed = true;
                        if (m_CurrentTween != null && m_CurrentTween.IsActive())
                            m_CurrentTween.Kill();
                        FinishHide();
                    }
                })
                .Forget();
        }

        private void FinishHide()
        {
            CLogger.LogInfo($"FinishHide called on {gameObject.name}", LogTag.Loading);
            gameObject.SetActive(false);
            m_CurrentTween = null;
            m_IsProcessingQueue = false;
            m_CompletedHideTransitions++;

            if (m_CompletedHideTransitions >= m_ActiveHideTransitions)
            {
                OnAllTransitionsHidden?.Invoke();
                m_CompletedHideTransitions = 0;
                m_ActiveHideTransitions = 0;
            }

            ProcessQueue();
        }

        public override void UpdateProgress(float progress) { }

        private void OnDestroy()
        {
            if (m_CurrentTween != null && m_CurrentTween.IsActive())
                m_CurrentTween.Kill();

            m_AnimationQueue.Clear();
        }

        [Header("Animation Settings")]
        [SerializeField]
        private float m_AnimationDuration = 0.5f;

        [SerializeField]
        private Ease m_EaseType = Ease.OutQuart;

        [SerializeField]
        private Vector3 m_ShowScale = Vector3.one;

        [SerializeField]
        private Vector3 m_HideScale = Vector3.zero;

        [SerializeField]
        private bool m_UseFade = true;

        private CanvasGroup m_CanvasGroup;
        private Tween m_CurrentTween;
        private Queue<System.Action> m_AnimationQueue = new Queue<System.Action>();
        private bool m_IsProcessingQueue = false;
        private static int m_ActiveHideTransitions = 0;
        private static int m_CompletedHideTransitions = 0;
        private static int m_ActiveShowTransitions = 0;
        private static int m_CompletedShowTransitions = 0;
    }
}
