using LDtkUnity;
using UnityEngine;

namespace Demos.LDtkDemo.RoomManager
{
    public class Portal : MonoBehaviour, ILDtkImportedEntity
    {
        private void Awake()
        {
            MyRoom = GetComponentInParent<Room>();

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider ??= gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }

        public void OnLDtkImportEntity(EntityInstance entityInstance)
        {
            MyInstanceId = entityInstance.Iid;

            LDtkFields fields = GetComponent<LDtkFields>();
            if (
                fields != null
                && fields.TryGetEntityReference("TargetTransition", out var targetRef)
            )
            {
                TargetTransitionIid = targetRef.EntityIid;
            }
        }

        public Vector3 GetArrivalPosition()
        {
            return transform.position;
        }

        private bool IsPlayer(Collider2D collider)
        {
            return collider.CompareTag("Player");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other))
                RoomManager.Instance.StartTransition(this);
        }


        public string MyInstanceId { get; private set; }
        public string TargetTransitionIid { get; private set; }
        public Room MyRoom { get; private set; }
    }
}
