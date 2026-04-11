using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityHFSM;

namespace Core
{
    public partial class GameCore : MonoSingletonPersistent<GameCore>
    {
        public void RegisterSystem(ICoreModuleSystem system)
        {
            m_Systems ??= new CoreSystemRegistry();
            m_Systems.Register(system);
        }

        public void Send(IGameFlowCommand command)
        {
            CLogger.LogInfo($"[GameCore] >> {command.CommandName}", LogTag.GameCoreStart);
            command.Execute().Forget();
        }

        public void RequestLoadLevel(string chapterId, string levelId, int spawnIndex = 0)
        {
            m_LoadContext = LoadContext.ForLevel(chapterId, levelId, spawnIndex);
            m_Fsm.Trigger(
                m_Fsm.ActiveStateName == GameFlowState.InLevel
                    ? GameFlowTrigger.SwitchLevel
                    : GameFlowTrigger.StartGame
            );
        }

        public void RequestLoadLevelFromSavePoint(string savePointName)
        {
            m_LoadContext = LoadContext.ForSavePoint(savePointName);
            m_Fsm.Trigger(
                m_Fsm.ActiveStateName == GameFlowState.InLevel
                    ? GameFlowTrigger.SwitchLevel
                    : GameFlowTrigger.StartGame
            );
        }

        public void RequestExitToMenu()
        {
            m_LoadContext = LoadContext.ForMainMenu();
            m_Fsm.Trigger(GameFlowTrigger.ExitToMenu);
        }

        internal async UniTask OnSceneEntryPointReady(SceneEntryPoint entry)
        {
            if (IsBootedFromEntry)
            {
                CLogger.LogInfo(
                    $"[GameCore] Scene ready (managed flow), state: {entry.TargetState}",
                    LogTag.GameCoreStart
                );
                return;
            }

            CLogger.LogInfo(
                $"[GameCore] EntryPoint detected: {entry.TargetState}",
                LogTag.GameCoreStart
            );

            if (entry.TargetState == GameFlowState.Booting)
            {
                IsBootedFromEntry = true;
                m_Fsm.RequestStateChange(GameFlowState.Booting, forceInstantly: true);
            }
            else
            {
                await RunStandaloneBootstrap(entry);
            }
        }

        internal void OnFlowUpdate() => m_Fsm.OnLogic();

        private void InitFlow()
        {
            m_Systems ??= new CoreSystemRegistry();
            m_Systems.OnBootStart(new ShowLogoStep().Execute, order: 10);
            m_Systems.OnBootStart(new LoadMainMenuSceneStep().Execute, order: 100);

            InitFsm();

            m_Fsm.SetStartState(GameFlowState.Booting);
            m_Fsm.Init();
        }

        private void InitFsm()
        {
            m_Fsm = new StateMachine<string, GameFlowState, GameFlowTrigger>();

            m_Fsm.AddState(GameFlowState.Booting, onEnter: _ => RunBooting().Forget());
            m_Fsm.AddState(
                GameFlowState.MainMenu,
                onEnter: _ =>
                {
                    CLogger.LogInfo("[GameCore] → MainMenu", LogTag.GameCoreStart);
                    m_Systems.FireOnMainMenuEnter().Forget();
                },
                onExit: _ =>
                {
                    CLogger.LogInfo("[GameCore] ← MainMenu", LogTag.GameCoreStart);
                    m_Systems.FireOnMainMenuExit().Forget();
                }
            );
            m_Fsm.AddState(GameFlowState.Loading, onEnter: _ => RunLoading().Forget());
            m_Fsm.AddState(
                GameFlowState.InLevel,
                onEnter: _ => SubscribeInLevelEvents(),
                onExit: _ => UnsubscribeInLevelEvents()
            );

            m_Fsm.AddTriggerTransition(
                GameFlowTrigger.BootComplete,
                GameFlowState.Booting,
                GameFlowState.MainMenu
            );
            m_Fsm.AddTriggerTransition(
                GameFlowTrigger.StartGame,
                GameFlowState.MainMenu,
                GameFlowState.Loading
            );
            m_Fsm.AddTriggerTransition(
                GameFlowTrigger.ExitToMenu,
                GameFlowState.InLevel,
                GameFlowState.Loading
            );
            m_Fsm.AddTriggerTransition(
                GameFlowTrigger.SwitchLevel,
                GameFlowState.InLevel,
                GameFlowState.Loading
            );
            m_Fsm.AddTriggerTransition(
                GameFlowTrigger.LoadComplete,
                GameFlowState.Loading,
                GameFlowState.InLevel,
                condition: _ => m_LoadContext.Destination == GameFlowState.InLevel
            );
            m_Fsm.AddTriggerTransition(
                GameFlowTrigger.LoadComplete,
                GameFlowState.Loading,
                GameFlowState.MainMenu,
                condition: _ => m_LoadContext.Destination == GameFlowState.MainMenu
            );
        }

        private async UniTask RunBooting()
        {
            CLogger.LogInfo("[GameCore] Booting...", LogTag.GameCoreStart);
            await m_Systems.InitializeAllAsync();
            await m_Systems.FireOnBootStart();
            m_Fsm.Trigger(GameFlowTrigger.BootComplete);
        }

        private async UniTask RunLoading()
        {
            CLogger.LogInfo("[GameCore] → Loading", LogTag.GameCoreStart);
            await m_Systems.FireOnLoadStart(m_LoadContext);
            await m_Systems.FireOnLoadScene(m_LoadContext);
            await m_Systems.FireOnLoadComplete(m_LoadContext);
            m_Fsm.Trigger(GameFlowTrigger.LoadComplete);
        }

        private void SubscribeInLevelEvents()
        {
            CLogger.LogInfo("[GameCore] → InLevel", LogTag.GameCoreStart);
            m_InLevelSubs = new DisposableBag();
            MessageBroker
                .Global.Subscribe<GameCoreEvents.LevelClearEvent>(OnLevelClear)
                .AddTo(ref m_InLevelSubs);

            m_Systems.FireOnInLevelEnter(m_LoadContext).Forget();
        }

        private void UnsubscribeInLevelEvents()
        {
            CLogger.LogInfo("[GameCore] ← InLevel", LogTag.GameCoreStart);
            m_InLevelSubs.Dispose();
            m_Systems.FireOnInLevelExit().Forget();
        }

        private void OnLevelClear(GameCoreEvents.LevelClearEvent e)
        {
            CLogger.LogInfo(
                $"Level Clear! Next: {e.NextChapterId}/{e.NextLevelId}",
                LogTag.GameCoreStart
            );
            RequestLoadLevel(e.NextChapterId, e.NextLevelId, 0);
        }

        private async UniTask RunStandaloneBootstrap(SceneEntryPoint entry)
        {
            CLogger.LogInfo(
                $"[GameCore] Standalone bootstrap for: {entry.TargetState}",
                LogTag.GameCoreStart
            );
            await m_Systems.InitializeAllAsync();
            m_LoadContext = entry.GetStandaloneContext();
            m_Fsm.RequestStateChange(GameFlowState.Loading, forceInstantly: true);
        }

        public GameFlowState CurrentState => m_Fsm.ActiveStateName;
        public bool IsBootedFromEntry { get; private set; }

        private StateMachine<string, GameFlowState, GameFlowTrigger> m_Fsm;
        private CoreSystemRegistry m_Systems;
        private LoadContext m_LoadContext;
        private DisposableBag m_InLevelSubs;
    }
}
