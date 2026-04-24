using Core;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace GameMain.RunTime
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CameraOverrideArea : AutoLdtkEntity
    {
        private void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;

            var go = new GameObject("OverrideCamera");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            m_OverrideCamera = go.AddComponent<CinemachineCamera>();
            go.AddComponent<CinemachinePositionComposer>();

            m_OverrideCamera.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            m_OverrideCamera.Lens.OrthographicSize = m_OrthographicSize;
            m_OverrideCamera.Lens.NearClipPlane = 0.1f;
            m_OverrideCamera.Lens.FarClipPlane = 5000f;

            m_OverrideCamera.Priority.Enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            m_OverrideCamera.Follow = other.transform;
            m_OverrideCamera.Priority.Enabled = true;
            m_OverrideCamera.Priority.Value = k_OverridePriority;

            CLogger.LogInfo(
                $"[CameraOverrideArea] {name}: camera override activated",
                LogTag.LevelManager
            );
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            m_OverrideCamera.Priority.Enabled = false;

            CLogger.LogInfo(
                $"[CameraOverrideArea] {name}: camera override deactivated",
                LogTag.LevelManager
            );
        }

        private void OnDisable()
        {
            if (m_OverrideCamera != null)
                m_OverrideCamera.Priority.Enabled = false;
        }

        private const int k_OverridePriority = 114514;

        [LabelText("镜头大小")]
        [SerializeField]
        private float m_OrthographicSize = 11.25f;

        private CinemachineCamera m_OverrideCamera;
    }
}
