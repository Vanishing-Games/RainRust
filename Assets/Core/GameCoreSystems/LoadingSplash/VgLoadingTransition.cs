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

        [Header("Animation Settings")]
        [SerializeField]
        private float animationDuration = 0.5f;

        [SerializeField]
        private Ease easeType = Ease.OutQuart;

        [SerializeField]
        private Vector3 showScale = Vector3.one;

        [SerializeField]
        private Vector3 hideScale = Vector3.zero;

        [SerializeField]
        private bool useFade = true;

        private CanvasGroup canvasGroup;
        private Tween currentTween;
        private Queue<System.Action> animationQueue = new Queue<System.Action>();
        private bool isProcessingQueue = false;
        private static int activeHideTransitions = 0;
        private static int completedHideTransitions = 0;
        private static int activeShowTransitions = 0;
        private static int completedShowTransitions = 0;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            // Ensure DOTween is initialized to avoid "Couldn't load Modules system" errors
            DOTween.Init();
        }

        public override void Show()
        {
            CLogger.LogInfo($"Show called on {gameObject.name}", LogTag.Loading);
            if (activeShowTransitions == 0 && completedShowTransitions == 0)
                ResetCounters();

            animationQueue.Enqueue(() => ExecuteShow());
            ProcessQueue();
        }

        public override void Hide()
        {
            if (activeHideTransitions == 0 && completedHideTransitions == 0)
                ResetCounters();

            animationQueue.Enqueue(() => ExecuteHide());
            ProcessQueue();
        }

        private static void ResetCounters()
        {
            activeHideTransitions = 0;
            completedHideTransitions = 0;
            activeShowTransitions = 0;
            completedShowTransitions = 0;
        }

        private void ProcessQueue()
        {
            if (isProcessingQueue || animationQueue.Count == 0)
                return;

            isProcessingQueue = true;
            var nextAction = animationQueue.Dequeue();
            nextAction?.Invoke();
        }

        private void ExecuteShow()
        {
            transform.localScale = hideScale;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (currentTween != null && currentTween.IsActive())
                currentTween.Kill();

            gameObject.SetActive(true);
            activeShowTransitions++;

            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOScale(showScale, animationDuration).SetEase(easeType));
            if (canvasGroup != null && useFade)
                seq.Join(
                    DOTween
                        .To(
                            () => canvasGroup.alpha,
                            x => canvasGroup.alpha = x,
                            1f,
                            animationDuration
                        )
                        .SetEase(easeType)
                );

            bool completed = false;
            currentTween = seq.OnComplete(() =>
            {
                if (completed)
                    return;
                completed = true;
                FinishShow();
            });

            // Fallback for DOTween errors
            UniTask
                .Delay(TimeSpan.FromSeconds(animationDuration + 0.1f))
                .ContinueWith(() =>
                {
                    if (!completed)
                    {
                        completed = true;
                        if (currentTween != null && currentTween.IsActive())
                            currentTween.Kill();
                        FinishShow();
                    }
                })
                .Forget();
        }

        private void FinishShow()
        {
            currentTween = null;
            isProcessingQueue = false;
            completedShowTransitions++;

            if (completedShowTransitions >= activeShowTransitions)
            {
                OnAllTransitionsShown?.Invoke();
                completedShowTransitions = 0;
                activeShowTransitions = 0;
            }

            ProcessQueue();
        }

        private void ExecuteHide()
        {
            if (currentTween != null && currentTween.IsActive())
                currentTween.Kill();

            activeHideTransitions++;

            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOScale(hideScale, animationDuration).SetEase(easeType));
            if (canvasGroup != null && useFade)
                seq.Join(
                    DOTween
                        .To(
                            () => canvasGroup.alpha,
                            x => canvasGroup.alpha = x,
                            0f,
                            animationDuration
                        )
                        .SetEase(easeType)
                );

            bool completed = false;
            currentTween = seq.OnComplete(() =>
            {
                if (completed)
                    return;
                completed = true;
                FinishHide();
            });

            // Fallback: If DOTween fails to start or complete (e.g. module error),
            // force completion after a safety margin.
            UniTask
                .Delay(TimeSpan.FromSeconds(animationDuration + 0.1f))
                .ContinueWith(() =>
                {
                    if (!completed)
                    {
                        completed = true;
                        if (currentTween != null && currentTween.IsActive())
                            currentTween.Kill();
                        FinishHide();
                    }
                })
                .Forget();
        }

        private void FinishHide()
        {
            CLogger.LogInfo($"FinishHide called on {gameObject.name}", LogTag.Loading);
            gameObject.SetActive(false);
            currentTween = null;
            isProcessingQueue = false;
            completedHideTransitions++;

            if (completedHideTransitions >= activeHideTransitions)
            {
                OnAllTransitionsHidden?.Invoke();
                completedHideTransitions = 0;
                activeHideTransitions = 0;
            }

            ProcessQueue();
        }

        public override void UpdateProgress(float progress) { }

        private void OnDestroy()
        {
            if (currentTween != null && currentTween.IsActive())
                currentTween.Kill();

            animationQueue.Clear();
        }
    }
}
