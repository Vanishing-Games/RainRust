using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameMain.RunTime
{
    public class SavePoint : MonoBehaviour
    {
        public string PointName;
        public string NickName;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CLogger.LogInfo(
                    $"Player reached save point: {PointName} ({NickName})",
                    LogTag.Game
                );
                SaveManager.Instance.WriteSlotSaveAsync().Forget();
            }
        }
    }
}
