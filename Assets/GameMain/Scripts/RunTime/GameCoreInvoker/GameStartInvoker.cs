using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class GameStartInvoker : MonoBehaviour
    {
        private void Start()
        {
            CLogger.LogInfo(
                "[GameStartInvoker] Start, Publishing GameStartInitEvent...",
                LogTag.GameCoreStart
            );
            MessageBroker.Global.Publish(new GameStartInitEvent());
        }
    }
}
