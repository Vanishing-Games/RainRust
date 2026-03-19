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
        }

        protected async void Start()
        {
            Logger.LogInfo("[GameCore] Start...", LogTag.GameCoreStart);

            if (!GameRunCheck())
            {
                Logger.LogInfo("[GameCore] Game Check Failed, Quit Game...", LogTag.GameRunCheck);
                await QuitGame();
                return;
            }

            // 检查当前场景，仅在 GameEntry 下启动初始化流程
            if (SceneManager.GetActiveScene().name == "GameEntry")
            {
                Logger.LogInfo(
                    "[GameCore] In GameEntry, Publishing GameEntryInitEvent...",
                    LogTag.GameCoreStart
                );
                MessageBroker.Global.Publish(new GameEntryInitEvent());
            }
            else
            {
                Logger.LogInfo(
                    $"[GameCore] Current Scene is {SceneManager.GetActiveScene().name}, skip entry initialization.",
                    LogTag.GameCoreStart
                );
            }
        }

        private void FixedUpdate() { }

        private void Update() { }

        private void OnDestroy()
        {
            Logger.LogInfo("[GameCore] OnDestroy...", LogTag.GameCoreDestroy);
            m_InitEventSubscription?.Dispose();
        }

        private IDisposable m_InitEventSubscription;
    }
}
