using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameMain.RunTime
{
    public static class GameMain
    {
        public static GameObject GetPlayer()
        {
            return GameObject.FindGameObjectWithTag("Player");
        }

        public static void SetPlayerFeetPosition(Vector3 feetPos)
        {
            var player = GetPlayer();
            var collider = player.GetComponent<BoxCollider2D>();
            float yOffset = collider.offset.y - collider.size.y / 2;
            feetPos += new Vector3(0,yOffset,0);

            player.transform.position = feetPos;
        }
    }
}
