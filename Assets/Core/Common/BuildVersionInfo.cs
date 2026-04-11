using UnityEngine;

namespace Core
{
    /// <summary>
    /// Provides access to build version information.
    /// This data is automatically generated during the build process.
    /// </summary>
    public static class BuildVersionInfo
    {
        private static BuildVersionData _cachedData;

        public static string Version => GetData().Version;
        public static string BuildTime => GetData().BuildTime;

        private static BuildVersionData GetData()
        {
            if (_cachedData == null)
            {
                var jsonAsset = Resources.Load<TextAsset>("BuildVersionInfo");
                if (jsonAsset != null)
                {
                    _cachedData = JsonUtility.FromJson<BuildVersionData>(jsonAsset.text);
                }
                else
                {
                    _cachedData = new BuildVersionData
                    {
                        Version = "v0.0.0-unknown",
                        BuildTime = "Unknown",
                    };
                }
            }
            return _cachedData;
        }

        [System.Serializable]
        private class BuildVersionData
        {
            public string Version;
            public string BuildTime;
        }
    }
}
