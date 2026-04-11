using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace GameMain.Editor.Tools
{
    /// <summary>
    /// 自动配置 Build Profiles 并生成占位素材的工具。
    /// </summary>
    public static class BuildProfileSetupTool
    {
        private const string MetaDataPath = "Assets/Arts/ProgramMetaData";
        private const string IconName = "art_programMeta_Icon.png";
        private const string SplashName = "art_programMeta_Splash.png";
        private const string CursorName = "art_programMeta_Cursor.png";
        private const string BuildProfilePath = "Assets/Settings/BuildProfiles";

        private const string CompanyName = "Vanishing Games";
        private const string ProductName = "Unity-CI-CD-Template";

        [MenuItem("Tools/Build/Auto Setup Profiles & Assets")]
        public static void Setup()
        {
            Debug.Log("开始自动配置 Build Profiles 和生成素材...");

            // 1. 生成素材
            Texture2D icon = GeneratePlaceholderTexture(1024, 1024, "ICON", Color.blue);
            Texture2D splash = GeneratePlaceholderTexture(
                1920,
                1080,
                "SPLASH SCREEN",
                new Color(0.1f, 0.1f, 0.1f)
            );
            Texture2D cursor = GeneratePlaceholderTexture(32, 32, "CURSOR", Color.green);

            string iconPath = SaveTexture(icon, IconName);
            string splashPath = SaveTexture(splash, SplashName);
            string cursorPath = SaveTexture(cursor, CursorName);

            AssetDatabase.Refresh();

            // 设置导入设置
            SetTextureToSprite(iconPath);
            SetTextureToSprite(splashPath);
            SetTextureToCursor(cursorPath);

            AssetDatabase.Refresh();

            // 2. 配置 Profiles
            SetupProfile(
                "Windows-Debug.asset",
                true,
                BuildTarget.StandaloneWindows64,
                iconPath,
                splashPath,
                cursorPath
            );
            SetupProfile(
                "Windows-Release.asset",
                false,
                BuildTarget.StandaloneWindows64,
                iconPath,
                splashPath,
                cursorPath
            );
            SetupProfile(
                "Mac-Debug.asset",
                true,
                BuildTarget.StandaloneOSX,
                iconPath,
                splashPath,
                cursorPath
            );
            SetupProfile(
                "Mac-Release.asset",
                false,
                BuildTarget.StandaloneOSX,
                iconPath,
                splashPath,
                cursorPath
            );

            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>Build Profiles 配置完成！</color>");
        }

        private static void SetupProfile(
            string fileName,
            bool isDebug,
            BuildTarget target,
            string iconPath,
            string splashPath,
            string cursorPath
        )
        {
            string fullPath = $"{BuildProfilePath}/{fileName}";
            BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(fullPath);

            if (profile == null)
            {
                Debug.LogError($"未找到 Build Profile: {fullPath}");
                return;
            }

            Undo.RecordObject(profile, "Setup Build Profile");

            // --- 1. 确保 PlayerSettings 覆盖层已开启 ---
            var utilType = typeof(BuildProfile).Assembly.GetType(
                "UnityEditor.Build.Profile.BuildProfileModuleUtil"
            );
            var hasSettingsMethod = utilType.GetMethod(
                "HasSerializedPlayerSettings",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            );
            if (
                hasSettingsMethod != null
                && !(bool)hasSettingsMethod.Invoke(null, new object[] { profile })
            )
            {
                var createMethod = utilType.GetMethod(
                    "CreatePlayerSettingsFromGlobal",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                );
                createMethod?.Invoke(null, new object[] { profile });
            }

            // --- 2. 宏定义与基础属性 ---
            HashSet<string> currentDefines = new HashSet<string>(profile.scriptingDefines);
            if (isDebug)
            {
                currentDefines.Add("BUILD_MODE_DEBUG");
                currentDefines.Add("ENABLE_DEBUG_LOG");
                currentDefines.Remove("BUILD_MODE_RELEASE");
            }
            else
            {
                currentDefines.Add("BUILD_MODE_RELEASE");
                currentDefines.Remove("BUILD_MODE_DEBUG");
                currentDefines.Remove("ENABLE_DEBUG_LOG");
            }
            profile.scriptingDefines = currentDefines.ToArray();
            profile.overrideGlobalScenes = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            // --- 3. 获取 GUIDs 和 FileIDs ---
            string iconGuid = AssetDatabase.AssetPathToGUID(iconPath);
            string cursorGuid = AssetDatabase.AssetPathToGUID(cursorPath);

            string splashGuid = "";
            long splashFileID = 2800000;
            Sprite splashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(splashPath);
            if (splashSprite != null)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    splashSprite,
                    out splashGuid,
                    out splashFileID
                );
            }

            // --- 4. 磁盘正则更新 (修复 PPtr Cast 错误) ---
            string diskPath = Path.GetFullPath(fullPath);
            string content = File.ReadAllText(diskPath);

            // 产品名与公司名
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(line: '\|   productName: )([^']+)",
                $"${{1}}{(isDebug ? ProductName + "_Debug" : ProductName)}"
            );
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(line: '\|   companyName: )([^']+)",
                $"${{1}}{CompanyName}"
            );

            // 调试开关
            string val = isDebug ? "1" : "0";
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(m_Development: )\d+",
                $"${{1}}{val}"
            );
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(m_AllowDebugging: )\d+",
                $"${{1}}{val}"
            );
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(m_CreateSolution: )\d+",
                $"${{1}}{val}"
            );
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(m_CopyPDBFiles: )\d+",
                $"${{1}}{val}"
            );
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(m_ConnectProfiler: )\d+",
                $"${{1}}{val}"
            );

            string texRef = $"{{fileID: 2800000, guid: {0}, type: 3}}";

            // 光标
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(line: '\|   defaultCursor: )([^']+)",
                $"${{1}}{texRef.Replace("{0}", cursorGuid)}"
            );

            // 闪屏 (Sprite)
            string splashRef = $"{{fileID: {splashFileID}, guid: {splashGuid}, type: 3}}";
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(line: '\|   - logo: )([^']+)",
                $"${{1}}{splashRef}"
            );
            if (content.Contains("m_SplashScreenLogos: []"))
            {
                string splashEntry =
                    "- line: '|   m_SplashScreenLogos:'\n    - line: '|   - logo: "
                    + splashRef
                    + "'\n    - line: '|     duration: 2'";
                content = content.Replace("- line: '|   m_SplashScreenLogos: []'", splashEntry);
            }

            // 图标 (Texture2D)
            string iconRef = texRef.Replace("{0}", iconGuid);
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(line: '\|       m_Icon: )([^']+)",
                $"${{1}}{iconRef}"
            );
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(line: '\|   - )(\{guid: [^}]+\})",
                $"${{1}}{iconRef}"
            );
            if (content.Contains("m_BuildTargetIcons: []"))
            {
                string iconEntry =
                    "- line: '|   m_BuildTargetIcons:'\n    - line: '|   - " + iconRef + "'";
                content = content.Replace("- line: '|   m_BuildTargetIcons: []'", iconEntry);
            }

            File.WriteAllText(diskPath, content);
            AssetDatabase.ImportAsset(fullPath);

            Debug.Log($"Profile {fileName} ({(isDebug ? "Debug" : "Release")}) 更新完成。");
        }

        private static Texture2D GeneratePlaceholderTexture(
            int width,
            int height,
            string text,
            Color bgColor
        )
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = bgColor;
            tex.SetPixels(pixels);
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, height / 2, Color.white);
            for (int y = 0; y < height; y++)
                tex.SetPixel(width / 2, y, Color.white);
            tex.Apply();
            return tex;
        }

        private static string SaveTexture(Texture2D tex, string fileName)
        {
            if (!Directory.Exists(MetaDataPath))
                Directory.CreateDirectory(MetaDataPath);
            string fullPath = Path.Combine(MetaDataPath, fileName);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            return fullPath;
        }

        private static void SetTextureToSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static void SetTextureToCursor(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Cursor;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
    }
}
