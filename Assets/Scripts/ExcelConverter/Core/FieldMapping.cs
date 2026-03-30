using System.Reflection;

namespace ExcelConverter.Core
{
    /// <summary>
    /// Excel 컬럼과 클래스 필드 간의 매핑 정보
    /// </summary>
    public class FieldMapping
    {
        public FieldInfo FieldInfo;
        public string ColumnName;
        public System.Type FieldType;
    }
}
