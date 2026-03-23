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
                "[GameLevelInvoker] Start, Publishing GameLevelInitEvent...",
                LogTag.GameCoreStart
            );
            MessageBroker.Global.Publish(new GameLevelInitEvent());
        }
    }
}
