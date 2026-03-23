using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.Editor
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

        private void Start()
        {
            if (AutoFindNearest)
            {
                new FastLevelTestCommand(transform.position).Execute();
            }
            else
            {
                new ManualFastLevelTestCommand(transform.position, SpawnIndex).Execute();
            }
        }
    }
}
