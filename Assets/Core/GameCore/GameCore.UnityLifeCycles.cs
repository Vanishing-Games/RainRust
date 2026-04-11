using UnityEngine;

namespace Core
{
    public partial class GameCore : MonoSingletonPersistent<GameCore>
    {
        protected override void Awake()
        {
            CLogger.LogInfo("[GameCore] Awake() called", LogTag.GameCoreStart);
            base.Awake();
            CLogger.LogInfo("[GameCore] Awake() done (InitFlow completed)", LogTag.GameCoreStart);
        }

        protected async void Start()
        {
            CLogger.LogInfo("[GameCore] Start...", LogTag.GameCoreStart);

            CLogger.LogInfo("[GameCore] Initing Flow", LogTag.GameCoreStart);
            InitFlow();
            CLogger.LogInfo("[GameCore] Flow initialized", LogTag.GameCoreStart);

            if (!GameRunCheck())
            {
                CLogger.LogInfo("[GameCore] Game Check Failed, Quit Game...", LogTag.GameRunCheck);
                await QuitGame();
            }
        }

        private void Update()
        {
            OnFlowUpdate();
            m_Systems.FireOnUpdate();
        }

        private void OnDestroy()
        {
            CLogger.LogInfo("[GameCore] OnDestroy...", LogTag.GameCoreDestroy);
        }
    }
}
