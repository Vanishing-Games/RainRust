using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 关卡结算点 / 游戏结束触发器
    /// 玩家碰到后会进入下一章节，或者进入制作人名单（目前为返回主菜单）
    /// </summary>
    public class LevelClearPoint : LDtkTriggerEntity
    {
        [Header("Config")]
        [LDtkField]
        public bool IsGameEnd = false;

        [LDtkField]
        public string NextSavePoint = "";

        private bool m_IsTriggered;

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (m_IsTriggered)
                return;

            // 检测是否是玩家触碰
            if (other.CompareTag("Player"))
            {
                m_IsTriggered = true;
                OnLevelClearTriggered();
            }
        }

        private void OnLevelClearTriggered()
        {
            if (IsGameEnd)
            {
                CLogger.LogInfo("Game End Triggered! Going to Credits/Main Menu.", LogTag.Game);

                // 目前没有独立的制作人名单界面，所以暂时调用 RequestExitToMenu
                // 后续如果有 Credits 流程，可在此抛出专门的 GameEnd 事件
                if (GameCore.Instance != null)
                {
                    GameCore.Instance.RequestExitToMenu();
                }
            }
            else
            {
                CLogger.LogInfo($"Level Clear! Loading Next: {NextSavePoint}", LogTag.Game);
                new LevelManagerCommands.StartLevelCommand(NextSavePoint).Execute();
            }
        }
    }
}
