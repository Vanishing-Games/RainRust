using UnityEngine;

namespace GameMain.RunTime
{
    public static class GameMain
    {
        public static GameObject GetPlayer()
        {
            return GameObject.FindGameObjectWithTag("Player");
        }
    }
}
