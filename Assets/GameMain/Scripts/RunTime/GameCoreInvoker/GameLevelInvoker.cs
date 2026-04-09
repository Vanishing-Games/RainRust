using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class GameLevelInvoker : MonoBehaviour
    {
        private void Start()
        {
            CLogger.LogInfo(
                "[GameLevelInvoker] Start, Publishing GameLevelPreInitEvent...",
                LogTag.GameCoreStart
            );

            MessageBroker.Global.Publish(new GameCoreInvokerEvents.GameCoreLevelPreInitEvent());
        }
    }
}
