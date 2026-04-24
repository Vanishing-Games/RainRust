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
        [LabelText("手动指定关卡")]
        public bool ManualLevel = false;

        [ShowIf("ManualLevel")]
        [LabelText("指定存档点")]
        public bool bySavePoint;

        [ShowIf("bySavePoint")]
        [LabelText("存档点名称")]
        public string savePointName;

        [ShowIf("ManualLevel")]
        [LabelText("关卡Id")]
        public string levelId;

        [ShowIf("ManualLevel")]
        [LabelText("章节Id")]
        public string chapterId;

        private void Start()
        {
            Vector3 testPos = transform.position;
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                testPos = player.transform.position;

            if (ManualLevel)
                if (bySavePoint)
                    new InitInvokerCommands.ManualFastLevelTestBySavePointCommand(savePointName);
                else
                    new InitInvokerCommands.ManualFastLevelTestCommand(
                        testPos,
                        chapterId,
                        levelId
                    ).Execute();
            else
                new InitInvokerCommands.FastLevelTestCommand(testPos).Execute();
        }
    }
}
