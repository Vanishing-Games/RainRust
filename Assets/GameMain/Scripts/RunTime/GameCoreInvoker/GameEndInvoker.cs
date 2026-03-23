using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class GameEndInvoker : MonoBehaviour
    {
        private void Start()
        {
            CLogger.LogInfo("[GameEndInvoker] Start, Publishing GameEndInitEvent...", LogTag.GameCoreStart);
            MessageBroker.Global.Publish(new GameEndInitEvent());
        }
    }
}
