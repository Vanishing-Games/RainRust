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
                "[GameStartInvoker] Start, Publishing GamePostInitEvent...",
                LogTag.GameCoreStart
            );

            MessageBroker.Global.Publish(new GameCoreEvents.GameCorePostInitEvent());
        }
    }
}
