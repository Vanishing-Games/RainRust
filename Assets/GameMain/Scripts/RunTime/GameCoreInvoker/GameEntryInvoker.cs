using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class GameEntryInvoker : MonoBehaviour
    {
        private void Start()
        {
            CLogger.LogInfo(
                "[GameEntryInvoker] Start, Publishing GamePreInitEvent...",
                LogTag.GameCoreStart
            );

            MessageBroker.Global.Publish(new GameCoreEvents.GameCorePreInitEvent());
        }
    }
}
