using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class VgLoadingCamera : MonoProgressable
    {
        public override void Hide()
        {
            HideAsync().Forget();
        }

        public override void Show()
        {
            ShowAsync().Forget();
        }

        private async UniTaskVoid HideAsync()
        {
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource = new CancellationTokenSource();

            try
            {
                await WaitForTransitionsToComplete(m_CancellationTokenSource.Token);
                VgCameraManager.Instance.SetLoadingCameraActive(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                m_CancellationTokenSource?.Dispose();
                m_CancellationTokenSource = null;
            }
        }

        private async UniTaskVoid ShowAsync()
        {
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource = new CancellationTokenSource();

            try
            {
                await WaitForTransitionsToComplete(m_CancellationTokenSource.Token);
                VgCameraManager.Instance.SetLoadingCameraActive(true);
            }
            catch (OperationCanceledException) { }
            finally
            {
                m_CancellationTokenSource?.Dispose();
                m_CancellationTokenSource = null;
            }
        }

        private async UniTask WaitForTransitionsToComplete(CancellationToken cancellationToken)
        {
            if (!HasActiveTransitions())
                return;

            await WaitForAllTransitionsHidden(cancellationToken);
        }

        private async UniTask WaitForAllTransitionsHidden(CancellationToken cancellationToken)
        {
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
                FindObjectsSortMode.None
            );
            return transitions.Length > 0;
        }

        public override void UpdateProgress(float progress) { }

        private CancellationTokenSource m_CancellationTokenSource;
    }
}
