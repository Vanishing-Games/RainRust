using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public interface IProgressable
    {
        void UpdateProgress(float progress);
        void Tick();
        void Show();
        void Hide();
    }

    public abstract class MonoProgressable : MonoBehaviour, IProgressable
    {
        public abstract void UpdateProgress(float progress);

        public virtual void Tick() { }

        public abstract void Show();
        public abstract void Hide();
    }

    public class VgLoadingSplashManager
        : CoreModuleManagerBase<VgLoadingSplashManager>,
            ICoreModuleSystem,
            IDisposable
    {
        public string SystemName => "VgLoadingSplashManager";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnLoadStart(_ => { Show(); return UniTask.CompletedTask; });
            registry.OnLoadComplete(_ => { Hide(); return UniTask.CompletedTask; });
            registry.OnBootStart(() => { Show(); return UniTask.CompletedTask; }, order: 0);
        }

        public void Init()
        {
            if (m_Inited)
                return;

            var progressables = FindObjectsByType<MonoProgressable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            foreach (var progressable in progressables)
                m_Progressables.Add(progressable);

            StringBuilder sb = new();
            sb.AppendLine("VgLoadingSplashManager Init with Progressables:");
            foreach (var progressable in m_Progressables)
                sb.AppendLine($" - {progressable.GetType()}");
            CLogger.LogInfo(sb.ToString(), LogTag.Loading);

            m_Inited = true;
        }

        public void Show()
        {
            if (!m_Inited)
                Init();

            foreach (var progressable in m_Progressables)
                progressable.Show();

            m_Hided = false;
        }

        public void UpdateProgress(float progress)
        {
            if (m_Hided)
            {
                CLogger.LogWarn(
                    "VgLoadingSplashManager is hidden, but UpdateProgress is called",
                    LogTag.Loading
                );
                Show();
            }

            if (progress < 0)
            {
                CLogger.LogWarn("Progress is less than 0, which is not allowed", LogTag.Loading);
                return;
            }

            progress = Mathf.Clamp(progress, 0f, 1f);

            foreach (var progressable in m_Progressables)
                progressable.UpdateProgress(progress);

            m_Progress = progress;
        }

        public void AddProgress(float progress) => UpdateProgress(m_Progress + progress);

        public float GetProgress() => m_Progress;

        public void Hide()
        {
            foreach (var progressable in m_Progressables)
                progressable.Hide();

            m_Hided = true;
        }

        public void Dispose() => Hide();

        private bool m_Inited = false;
        private bool m_Hided = true;
        private List<IProgressable> m_Progressables = new();
        private float m_Progress = 0;
    }
}
