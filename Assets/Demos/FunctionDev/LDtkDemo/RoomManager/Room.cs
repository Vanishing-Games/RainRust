using demo_ldtk_enum;
using LDtkUnity;
using Unity.Cinemachine;
using UnityEngine;

namespace Demos.LDtkDemo.RoomManager
{
    public class Room : MonoBehaviour, ILDtkImportedLevel
    {
        public void OnLDtkImportLevel(Level level)
        {
            MyInstanceId = level.Iid;
            Identifier = level.Identifier;
            Size = new Vector2(level.PxWid, level.PxHei);
            WorldRect = new Rect(
                level.WorldX,
                -level.WorldY - level.PxHei,
                level.PxWid,
                level.PxHei
            );

            if (TryGetComponent<LDtkFields>(out var fields))
                CameraMode = fields.GetEnum<CameraMode>("CameraMode");
        }

        public void Initialize(Transform player)
        {
            virtualCamera ??= GetComponentInChildren<CinemachineCamera>();

            if (virtualCamera != null)
            {
                if (CameraMode == CameraMode.Fixed)
                {
                    virtualCamera.Follow = null;
                    virtualCamera.transform.position = new Vector3(
                        WorldRect.center.x,
                        WorldRect.center.y,
                        -10
                    );

                    float roomAspect = Size.x / Size.y;
                    float screenAspect = (float)Screen.width / Screen.height;
                    float targetOrthoSize = Size.y / 2f / 100f;

                    if (roomAspect > screenAspect)
                    {
                        targetOrthoSize = Size.x / screenAspect / 2f / 100f;
                    }

                    virtualCamera.Lens.OrthographicSize = targetOrthoSize;
                }
                else
                {
                    virtualCamera.Follow = player;
                }
            }

            if (confiner != null)
            {
                if (!TryGetComponent<PolygonCollider2D>(out var poly))
                    poly = GetComponentInChildren<PolygonCollider2D>();

                if (poly != null)
                {
                    confiner.BoundingShape2D = poly;
                }
            }
        }

        public void SetActive(bool active)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Priority = active ? 10 : 0;
            }
            gameObject.SetActive(active);
        }

        [Header("LDtk Data")]
        public string MyInstanceId { get; private set; }
        public string Identifier { get; private set; }
        public CameraMode CameraMode { get; private set; }
        public Vector2 Size { get; private set; }
        public Rect WorldRect { get; private set; }

        [Header("Cinemachine")]
        public CinemachineCamera virtualCamera;
        public CinemachineConfiner2D confiner;
    }
}
