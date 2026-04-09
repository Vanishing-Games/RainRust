using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class CoreModuleLoaderEvents
    {
        public enum LoadMode
        {
            LoadOverwrite,
            LoadAdditively,
        }

        public class LoadRequestEvent : IEvent
        {
            public struct LoadSettings
            {
                public UInt32 maxWaitTimeInMs;
                public LoadMode loadMode;

                public static LoadSettings Default =>
                    new() { maxWaitTimeInMs = 1000, loadMode = LoadMode.LoadOverwrite };
            }

            public LoadRequestEvent(string loadDesc)
            {
                m_LoadDesc = loadDesc;
            }

            public LoadRequestEvent(
                string loadEventType,
                List<ILoadInfo> loadInfo,
                LoadSettings loadSettings
            )
            {
                m_LoadDesc = loadEventType;
                m_LoadInfos = loadInfo;
                m_LoadSettings = loadSettings;
            }

            public void Set(LoadSettings loadSettings)
            {
                m_LoadSettings = loadSettings;
            }

            public void AddLoadInfo(ILoadInfo loadInfo)
            {
                m_LoadInfos.Add(loadInfo);
            }

            public ILoadInfo GetLoadInfo(LoaderType loaderType)
            {
                return m_LoadInfos.FirstOrDefault(loadInfo =>
                    loadInfo.GetNeededLoaderType() == loaderType
                );
            }

            public List<ILoadInfo> m_LoadInfos { get; private set; } = new();
            public LoadSettings m_LoadSettings { get; set; } = LoadSettings.Default;
            public string m_LoadDesc { get; private set; } = string.Empty;
        }

        public class LoadPostStartEvent : IEvent { }

        public class LoadProgressEvent : IEvent
        {
            public LoadProgressEvent(string progress)
            {
                Progress = progress;
            }

            public string Progress { get; private set; }
        }
    }
}
