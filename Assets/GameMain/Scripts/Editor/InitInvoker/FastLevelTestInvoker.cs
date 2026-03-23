using UnityEngine;

namespace GameMain.Editor
{
    /// <summary>
    /// 快速关卡测试
    /// 将逻辑封装在 FastLevelTestCommand 中执行
    /// </summary>
    public class FastLevelTestInvoker : MonoBehaviour
    {
        private void Start()
        {
            new FastLevelTestCommand(transform.position).Execute();
        }
    }
}
