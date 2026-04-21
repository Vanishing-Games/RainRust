using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class VgLoadingBackground : MonoProgressable
    {
        public override void Hide()
        {
            HideAsync().Forget();
        }

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        private async UniTaskVoid HideAsync()
        {
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource = new CancellationTokenSource();

            try
            {
                await WaitForAllTransitionsHidden(m_CancellationTokenSource.Token);
                gameObject.SetActive(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                m_CancellationTokenSource?.Dispose();
                m_CancellationTokenSource = null;
            }
        }

        private async UniTask WaitForAllTransitionsHidden(CancellationToken cancellationToken)
        {
            if (!HasActiveTransitions())
                return;

            var tcs = new UniTaskCompletionSource();
            var isCompleted = false;

            void OnTransitionsHidden()
            {
                if (!isCompleted)
                {
                    isCompleted = true;
                    VgLoadingTransition.OnAllTransitionsHidden -= OnTransitionsHidden;
                    tcs.TrySetResult();
                }
            }

            VgLoadingTransition.OnAllTransitionsHidden += OnTransitionsHidden;

            try
            {
                await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!isCompleted)
                {
                    VgLoadingTransition.OnAllTransitionsHidden -= OnTransitionsHidden;
                }
                throw;
            }
        }

        private bool HasActiveTransitions()
        {
            VgLoadingTransition[] transitions = FindObjectsByType<VgLoadingTransition>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            return transitions.Length > 0;
        }

        public override void UpdateProgress(float progress) { }

        private CancellationTokenSource m_CancellationTokenSource;
    }
}
