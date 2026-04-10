using System;
using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public partial class GameCore : MonoSingletonPersistent<GameCore>
    {
        protected override void Awake()
        {
            base.Awake();

            m_InitEventSubscription =
                MessageBroker.Global.Subscribe<GameCoreInvokerEvents.GameCorePreInitEvent>(_ =>
                {
                    new GameEntryInitCommand().Execute();
                });

            m_StartEventSubscription =
                MessageBroker.Global.Subscribe<GameCoreInvokerEvents.GameCorePostInitEvent>(_ =>
                {
                    // TODO: Handle GamePostInitEvent if needed
                });

            m_LevelEventSubscription =
                MessageBroker.Global.Subscribe<GameCoreInvokerEvents.GameCoreLevelPreInitEvent>(_ =>
                {
                    // TODO: Handle GameLevelPreInitEvent if needed
                });

            m_EndEventSubscription =
                MessageBroker.Global.Subscribe<GameCoreInvokerEvents.GameCorePostEndEvent>(_ =>
                {
                    // TODO: Handle GamePostEndEvent if needed
                });

            m_CustomEventSubscription =
                MessageBroker.Global.Subscribe<GameCoreEvents.GameCoreCustomInitEvent>(_ =>
                {
                    // TODO: Handle GameCustomInitEvent if needed
                });
        }

        protected async void Start()
        {
            CLogger.LogInfo("[GameCore] Start...", LogTag.GameCoreStart);

            if (!GameRunCheck())
            {
                CLogger.LogInfo("[GameCore] Game Check Failed, Quit Game...", LogTag.GameRunCheck);
                await QuitGame();
                return;
            }
        }

        private void FixedUpdate() { }

        private void Update() { }

        private void OnDestroy()
        {
            CLogger.LogInfo("[GameCore] OnDestroy...", LogTag.GameCoreDestroy);
            m_InitEventSubscription?.Dispose();
            m_StartEventSubscription?.Dispose();
            m_LevelEventSubscription?.Dispose();
            m_EndEventSubscription?.Dispose();
            m_CustomEventSubscription?.Dispose();
        }

        private IDisposable m_InitEventSubscription;
        private IDisposable m_StartEventSubscription;
        private IDisposable m_LevelEventSubscription;
        private IDisposable m_EndEventSubscription;
        private IDisposable m_CustomEventSubscription;
    }
}
