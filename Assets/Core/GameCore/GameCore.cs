using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Core
{
    public partial class GameCore : MonoSingletonPersistent<GameCore>
    {
        private bool GameRunCheck()
        {
            CLogger.LogInfo("[GameCore] Runing Game Check...", LogTag.GameRunCheck);

#if UNITY_EDITOR
            if (!GameRunInEditorCheck())
                return false;
#endif
            return true;
        }

        internal async UniTask QuitGame()
        {
            try
            {
                CLogger.LogInfo("[GameCore] Saving game before quit...", LogTag.GameQuit);
                
                // Save slot and global data before quitting
                await SaveManager.Instance.WriteSlotSaveAsync().Timeout(TimeSpan.FromSeconds(5));
                await SaveManager.Instance.WriteGlobalSaveAsync().Timeout(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                CLogger.LogError($"Error during save operation: {ex.Message}", LogTag.GameQuit);
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
