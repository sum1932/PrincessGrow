using System;

namespace ExcelConverter.Attributes
{
    /// <summary>
    /// 필드명과 Excel 컬럼명이 다를 때 매핑을 지정합니다.
    /// 지정하지 않으면 필드명과 동일한 컬럼명을 찾습니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcelColumnAttribute : Attribute
    {
        public string Name;

        public ExcelColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
