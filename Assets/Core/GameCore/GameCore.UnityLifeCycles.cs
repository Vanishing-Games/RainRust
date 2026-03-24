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

            m_InitEventSubscription = MessageBroker.Global.Subscribe<GameEntryInitEvent>(_ =>
            {
                new GameEntryInitCommand().Execute();
            });

            m_StartEventSubscription = MessageBroker.Global.Subscribe<GameStartInitEvent>(_ =>
            {
                // TODO: Handle GameStartInitEvent if needed
            });

            m_LevelEventSubscription = MessageBroker.Global.Subscribe<GameLevelInitEvent>(_ =>
            {
                // TODO: Handle GameLevelInitEvent if needed
            });

            m_EndEventSubscription = MessageBroker.Global.Subscribe<GameEndInitEvent>(_ =>
            {
                // TODO: Handle GameEndInitEvent if needed
            });

            m_CustomEventSubscription = MessageBroker.Global.Subscribe<GameCustomInitEvent>(_ =>
            {
                // TODO: Handle GameEndInitEvent if needed
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
        }

        private IDisposable m_InitEventSubscription;
        private IDisposable m_StartEventSubscription;
        private IDisposable m_LevelEventSubscription;
        private IDisposable m_EndEventSubscription;
        private IDisposable m_CustomEventSubscription;
    }
}
