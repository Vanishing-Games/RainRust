using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    public class PlayerManager : MonoBehaviour
    {
        public void PlacePlayer(Vector3 position)
        {
            GetPlayer();

            if (m_Player == null)
            {
                Core.CLogger.LogError(
                    "Player GameObject not found in the scene. Please make sure there is a GameObject tagged 'Player'.",
                    LogTag.PlayerManager
                );
                return;
            }

            m_Player.transform.position = position;
        }

        private void GetPlayer()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");

            if (m_Player != null && players.Length == 1 && players[0] == m_Player)
                return;

            if (players.Length > 0)
                Core.CLogger.LogError(
                    "Something went wrong, there should be only one player in the scene, but found "
                        + players.Length,
                    LogTag.PlayerManager
                );

            m_Player = players.Length > 0 ? players[0] : null;
        }

        private GameObject m_Player;
    }
}
