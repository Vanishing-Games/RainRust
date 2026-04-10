using System;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameMain.RunTime
{
    public class PlayerManageSystem : CoreModuleManagerBase<PlayerManageSystem>, ICoreModuleSystem
    {
        public string SystemName => "PlayerManageSystem";
        public Type[] Dependencies => new[] { typeof(LevelManager) };

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnInLevelEnter(async ctx => await SpawnPlayer(ctx));
            registry.OnInLevelExit(async () => DespawnPlayer());
        }

        public async UniTask SpawnPlayer(LoadContext ctx)
        {
            GetPlayer();
            if (m_Player == null)
            {
                CLogger.LogError(
                    "Player GameObject not found in the scene. Please make sure there is a GameObject tagged 'Player'.",
                    LogTag.Loading
                );
            }
            await UniTask.CompletedTask;
        }

        public void DespawnPlayer()
        {
            m_Player = null;
        }

        public void PlacePlayer(Vector3 position)
        {
            GetPlayer();

            if (m_Player == null)
            {
                CLogger.LogError(
                    "Player GameObject not found in the scene. Please make sure there is a GameObject tagged 'Player'.",
                    LogTag.Loading
                );
                return;
            }

            m_Player.transform.position = position;
        }

        private void GetPlayer()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");

            if (m_Player != null && players.Length == 1 && players[0] == m_Player)
            {
                return;
            }

            if (players.Length > 1)
            {
                CLogger.LogError(
                    "Something went wrong, there should be only one player in the scene, but found "
                        + players.Length,
                    LogTag.Loading
                );
            }

            m_Player = players.Length > 0 ? players[0] : null;
        }

        private GameObject m_Player;
    }
}
