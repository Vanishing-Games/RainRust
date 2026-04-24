using UnityEngine;

namespace GameMain.RunTime
{
    public class MovePlatformerPathPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
