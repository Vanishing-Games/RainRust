using UnityEngine;

namespace GameMain.RunTime
{
    public class PlayerPlatformerInitializer : MonoBehaviour, IPlayer
    {
        public void Initialize(LevelManager.LevelLoadInfo loadInfo)
        {
            gameObject.transform.position = loadInfo.SpawnPosition;
        }
    }
}
