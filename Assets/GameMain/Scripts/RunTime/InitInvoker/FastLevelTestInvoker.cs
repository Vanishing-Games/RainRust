using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// 快速关卡测试
    /// 将逻辑封装在 FastLevelTestCommand 中执行
    /// </summary>
    public class FastLevelTestInvoker : MonoBehaviour
    {
        [Title("快速关卡测试配置")]
        [LabelText("自动寻找最近出生点")]
        public bool AutoFindNearest = true;

        [HideIf("AutoFindNearest")]
        [LabelText("出生点 Index")]
        public int SpawnIndex = 0;

        [HideIf("AutoFindNearest")]
        [LabelText("关卡Id")]
        public string levelId;

        [HideIf("AutoFindNearest")]
        [LabelText("章节Id")]
        public string chapterId;

        private void Start()
        {
            // 优先获取场景中 Player 的位置作为测试参考点
            Vector3 testPos = transform.position;
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                testPos = player.transform.position;
            }

            if (AutoFindNearest)
            {
                new InitInvokerCommands.FastLevelTestCommand(testPos).Execute();
            }
            else
            {
                new InitInvokerCommands.ManualFastLevelTestCommand(
                    testPos,
                    chapterId,
                    levelId,
                    SpawnIndex
                ).Execute();
            }
        }
    }
}
