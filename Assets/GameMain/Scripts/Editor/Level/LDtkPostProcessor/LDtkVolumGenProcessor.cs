using System.IO;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameMain.Editor
{
    public class LDtkVolumeGenProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 5;

        // Unity AssetDatabase 必须使用相对路径
        private const string PROFILE_FOLDER_PATH =
            "Assets/GameMain/LDtkProject/gm_ldtk_project/VolumeProfiles";

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            if (!root.TryGetComponent<LDtkComponentLevel>(out var level))
                return;

            string volumeName = $"Volume_{level.Identifier}";

            CLogger.LogInfo(
                $"Generating/Updating Local Volume for level: {level.Identifier}",
                LogTag.LDtkVolumGenProcessor
            );

            Transform existingVolume = root.transform.Find(volumeName);
            GameObject volumeGo;
            if (existingVolume != null)
            {
                volumeGo = existingVolume.gameObject;
            }
            else
            {
                volumeGo = new GameObject(volumeName);
                volumeGo.transform.SetParent(root.transform);
            }

            volumeGo.transform.localPosition = Vector3.zero;
            volumeGo.layer = LayerMask.NameToLayer("Volume");

            if (!volumeGo.TryGetComponent(out Volume volume))
            {
                volume = volumeGo.AddComponent<Volume>();
            }
            volume.isGlobal = false;
            volume.blendDistance = 1f;

            if (!volumeGo.TryGetComponent(out BoxCollider boxCollider))
            {
                boxCollider = volumeGo.AddComponent<BoxCollider>();
            }
            boxCollider.isTrigger = true;

            Vector2 size = level.Size;
            boxCollider.center = new Vector3(size.x / 2f, size.y / 2f, 0f);
            boxCollider.size = new Vector3(size.x, size.y, 50f);

            string profilePath = $"{PROFILE_FOLDER_PATH}/{volumeName}.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

            if (profile == null)
            {
                if (!Directory.Exists(PROFILE_FOLDER_PATH))
                {
                    Directory.CreateDirectory(PROFILE_FOLDER_PATH);
                    AssetDatabase.Refresh();
                }

                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = volumeName;
                AssetDatabase.CreateAsset(profile, profilePath);
                AssetDatabase.SaveAssets();

                CLogger.LogInfo(
                    $"Created new Volume Profile at {profilePath}",
                    LogTag.LDtkVolumGenProcessor
                );
            }

            volume.sharedProfile = profile;

            EditorUtility.SetDirty(volumeGo);
        }
    }
}
