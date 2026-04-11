using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.BuildScripts
{
    public class GitVersionProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            string version = GetGitVersion();
            PlayerSettings.bundleVersion = version;
            
            // For Android/iOS
            string buildNumberStr = GetBuildNumber();
            if (int.TryParse(buildNumberStr, out int buildNumber))
            {
                PlayerSettings.Android.bundleVersionCode = buildNumber;
                PlayerSettings.iOS.buildNumber = buildNumberStr;
            }

            Debug.Log($"[BuildVersion] Current version: {version}");
            GenerateVersionInfoFile(version);
        }

        private string GetGitVersion()
        {
            // Check CI/CD environment variables first (e.g., from GitHub Actions)
            string ciVersion = Environment.GetEnvironmentVariable("BUILD_VERSION");
            if (!string.IsNullOrEmpty(ciVersion)) return ciVersion;

            try
            {
                ProcessStartInfo info = new ProcessStartInfo("git", "describe --tags --always --dirty")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = Process.Start(info))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    return string.IsNullOrEmpty(output) ? "0.1.0-unknown" : output;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildVersion] Failed to get git version: {ex.Message}");
                return "0.1.0-no-git";
            }
        }

        private string GetBuildNumber()
        {
            // GitHub Actions provides GITHUB_RUN_NUMBER
            string ciBuildNumber = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
            if (!string.IsNullOrEmpty(ciBuildNumber)) return ciBuildNumber;

            // Fallback: Date-based build number (integer for store compatibility)
            return DateTime.Now.ToString("yyyyMMdd");
        }

        private void GenerateVersionInfoFile(string version)
        {
            string filePath = "Assets/Core/Common/BuildVersionInfo.cs";
            string content = $@"/*
 * AUTO-GENERATED FILE - DO NOT MODIFY
 */
namespace Core
{{
    public static class BuildVersionInfo
    {{
        public const string Version = ""{version}"";
        public const string BuildTime = ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}"";
    }}
}}";
            try
            {
                System.IO.File.WriteAllText(filePath, content);
                AssetDatabase.ImportAsset(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildVersion] Failed to write version info: {ex.Message}");
            }
        }
    }
}
