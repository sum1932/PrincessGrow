namespace ExcelConverter.Core
{
    /// <summary>
    /// 데이터 타입별 Excel 파일 정보
    /// </summary>
    public class ExcelFileInfo
    {
        public string FileName;
        public string SheetName;

        public ExcelFileInfo(string fileName, string sheetName)
        {
            FileName = fileName;
            SheetName = sheetName;
        }
    }
}
