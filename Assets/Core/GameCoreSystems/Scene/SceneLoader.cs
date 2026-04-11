using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Core
{
    public class SceneLoader
    {
        public async UniTask LoadScene(SceneLoadInfo info)
        {
            m_SceneLoadInfo = info;
            await (
                info.SceneAssetMode switch
                {
                    SceneAssetMode.FromBuildingSceneName => LoadSceneFromBuildingName(),
                    SceneAssetMode.FromBuildingSceneIndex => LoadSceneFromBuildingIndex(),
                    SceneAssetMode.FromEditorPath => LoadSceneFromEditorPath(),
                    SceneAssetMode.FromStreamingAssetsPath => LoadSceneFromStreamingAssetPath(),
                    _ => throw new InvalidOperationException(
                        $"Unsupported scene asset mode: {info.SceneAssetMode}"
                    ),
                }
            );
        }

        public async UniTask UnloadAddressableScene()
        {
            if (m_AddressableSceneHandle.IsValid())
            {
                await Addressables.UnloadSceneAsync(m_AddressableSceneHandle).ToUniTask();
                m_AddressableSceneHandle = default;
            }
        }

        private async UniTask LoadSceneFromBuildingName()
        {
            if (string.IsNullOrEmpty(m_SceneLoadInfo.SceneName))
                throw new InvalidOperationException("Scene name is null or empty");

            var asyncOp = SceneManager.LoadSceneAsync(
                m_SceneLoadInfo.SceneName,
                m_SceneLoadInfo.SceneLoadParameters
            );
            if (asyncOp == null)
                throw new InvalidOperationException("Failed to start loading scene by name");

            await asyncOp;
        }

        private async UniTask LoadSceneFromBuildingIndex()
        {
            if (m_SceneLoadInfo.SceneIndex < 0)
                throw new InvalidOperationException(
                    $"Invalid scene index: {m_SceneLoadInfo.SceneIndex}"
                );

            var asyncOp = SceneManager.LoadSceneAsync(
                m_SceneLoadInfo.SceneIndex,
                m_SceneLoadInfo.SceneLoadParameters
            );
            if (asyncOp == null)
                throw new InvalidOperationException("Failed to start loading scene by index");

            await asyncOp;
        }

        private async UniTask LoadSceneFromEditorPath()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(m_SceneLoadInfo.EditorPath))
                throw new InvalidOperationException("Editor path is null or empty");

            if (!System.IO.File.Exists(m_SceneLoadInfo.EditorPath))
            {
                throw new InvalidOperationException(
                    $"Scene file not found at path: {m_SceneLoadInfo.EditorPath}"
                );
            }
            await UniTask.Yield();
#else
            throw new InvalidOperationException(
                "Editor path loading is only supported in Unity Editor"
            );
#endif
        }

        private async UniTask LoadSceneFromStreamingAssetPath()
        {
            if (string.IsNullOrEmpty(m_SceneLoadInfo.StreamingAssetPath))
                throw new InvalidOperationException("Streaming asset path is null or empty");
            try
            {
                var addressableKey = m_SceneLoadInfo.StreamingAssetPath;
                var sceneHandle = Addressables.LoadSceneAsync(
                    addressableKey,
                    m_SceneLoadInfo.SceneLoadParameters.loadSceneMode,
                    m_SceneLoadInfo.SceneLoadParameters.localPhysicsMode != LocalPhysicsMode.None
                );

                var sceneInstance = await sceneHandle.ToUniTask();
                if (!sceneInstance.Scene.isLoaded)
                {
                    throw new InvalidOperationException(
                        $"Scene failed to load from Addressables: {addressableKey}"
                    );
                }
                m_AddressableSceneHandle = sceneHandle;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load scene from Addressables: {m_SceneLoadInfo.StreamingAssetPath}",
                    ex
                );
            }
        }

        private SceneLoadInfo m_SceneLoadInfo;
        private AsyncOperationHandle<SceneInstance> m_AddressableSceneHandle;
    }
}
