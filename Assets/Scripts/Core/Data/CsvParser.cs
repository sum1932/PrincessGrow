using System;
using System.Collections.Generic;
using System.Linq;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// CSV 파싱 유틸리티
    /// </summary>
    public static class CsvParser
    {
        /// <summary>
        /// CSV 텍스트를 헤더와 데이터 행으로 파싱
        /// </summary>
        public static List<Dictionary<string, string>> Parse(string csvText, char delimiter = ',')
        {
            var result = new List<Dictionary<string, string>>();
            
            // ★ UTF-8 BOM 제거 (한글 깨짐 방지)
            if (csvText.Length > 0 && csvText[0] == '\uFEFF')
            {
                csvText = csvText.Substring(1);
            }
            
            var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length == 0) return result;
            
            // 첫 줄: 헤더
            var headers = ParseLine(lines[0], delimiter);
            
            // 데이터 행
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                
                var values = ParseLine(line, delimiter);
                if (values.Count == 0) continue;
                
                var row = new Dictionary<string, string>();
                for (int j = 0; j < headers.Count && j < values.Count; j++)
                {
                    row[headers[j]] = values[j];
                }
                result.Add(row);
            }
            
            return result;
        }
        
        /// <summary>
        /// CSV 한 줄 파싱 (쉼표와 따옴표 처리)
        /// </summary>
        private static List<string> ParseLine(string line, char delimiter)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    // 따옴표 처리
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 이중 따옴표
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    // 구분자
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            
            // 마지막 필드
            result.Add(current.ToString().Trim());
            return result;
        }
        
        /// <summary>
        /// 문자열을 int로 변환 (실패 시 기본값)
        /// </summary>
        public static int GetInt(Dictionary<string, string> row, string key, int defaultValue = 0)
        {
            if (row.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            {
                if (int.TryParse(value, out int result))
                    return result;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 문자열을 bool로 변환
        /// </summary>
        public static bool GetBool(Dictionary<string, string> row, string key, bool defaultValue = false)
        {
            if (row.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            {
                if (bool.TryParse(value, out bool result))
                    return result;
                // TRUE/FALSE 대문자 처리
                return value.ToUpper() == "TRUE" || value == "1" || value.ToUpper() == "YES";
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 문자열 가져오기
        /// </summary>
        public static string GetString(Dictionary<string, string> row, string key, string defaultValue = "")
        {
            return row.TryGetValue(key, out string value) ? value : defaultValue;
        }
        
        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T GetEnum<T>(Dictionary<string, string> row, string key, T defaultValue) where T : struct
        {
            if (row.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            {
                if (System.Enum.TryParse<T>(value, true, out T result))
                    return result;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 쉼표로 구분된 문자열을 리스트로 변환
        /// </summary>
        public static List<string> GetList(Dictionary<string, string> row, string key, char separator = ';')
        {
            var result = new List<string>();
            if (row.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            {
                result = value.Split(separator)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
            return result;
        }
    }
}
