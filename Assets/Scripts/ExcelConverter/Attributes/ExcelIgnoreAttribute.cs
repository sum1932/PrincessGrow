using System;

namespace ExcelConverter.Attributes
{
    /// <summary>
    /// Excel 변환에서 제외할 필드를 지정합니다.
    /// 런타임 계산 필드나 임시 필드에 사용합니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcelIgnoreAttribute : Attribute { }
}
