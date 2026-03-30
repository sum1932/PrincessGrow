using System;

namespace ExcelConverter.Attributes
{
    /// <summary>
    /// Excel 데이터의 ID(PK) 필드를 지정합니다.
    /// 각 데이터 클래스에 반드시 하나의 필드에 적용되어야 합니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcelIdAttribute : Attribute { }
}
