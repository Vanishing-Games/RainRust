using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    [RequireComponent(typeof(PlayerSnake))]
    public class PlayerSnakeInitializer : MonoBehaviour, IPlayer
    {
        public void Initialize(LevelManager.LevelLoadInfo loadInfo)
        {
            gameObject.GetComponent<PlayerSnake>().SetUp(loadInfo.SpawnPosition);
        }
    }
}
