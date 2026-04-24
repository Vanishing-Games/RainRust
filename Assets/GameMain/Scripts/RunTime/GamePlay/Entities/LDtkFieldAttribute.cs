using System;

namespace GameMain.RunTime
{
    /// <summary>
    /// 用于标记需要从 LDtk 字段自动映射的字段或属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LDtkFieldAttribute : Attribute
    {
        public string CustomIdentifier { get; }

        public LDtkFieldAttribute(string identifier = null)
        {
            CustomIdentifier = identifier;
        }
    }
}
