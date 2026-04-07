using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExcelConverter.Core
{
    /// <summary>
    /// CSV 파일을 읽어 데이터 행을 제공하는 리더
    /// </summary>
    public class CsvSheetReader : IDisposable
    {
        public string[] Headers { get; private set; }
        private readonly List<Dictionary<string, string>> _rows = new();

        public CsvSheetReader(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvPath}");
                Headers = Array.Empty<string>();
                return;
            }

            try
            {
                var lines = File.ReadAllLines(csvPath);
                if (lines.Length == 0)
                {
                    Headers = Array.Empty<string>();
                    return;
                }

                // 첫 줄은 헤더
                Headers = ParseCsvLine(lines[0]);
                
                // 데이터 행 파싱 (주석 행 제외)
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    // 빈 줄이나 주석(#)은 스킵
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    var values = ParseCsvLine(line);
                    if (values.Length == 0) continue;

                    var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int j = 0; j < Headers.Length && j < values.Length; j++)
                    {
                        row[Headers[j]] = values[j]?.Trim() ?? string.Empty;
                    }
                    _rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"CSV 파일 읽기 실패: {csvPath}\n{ex.Message}");
                Headers = Array.Empty<string>();
            }
        }

        /// <summary>
        /// CSV 라인을 파싱 (쉼표와 따옴표 처리)
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 이스케이프된 따옴표
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// 모든 데이터 행을 반환
        /// </summary>
        public IEnumerable<DataRow> GetDataRows()
        {
            foreach (var row in _rows)
            {
                yield return new DataRow(row);
            }
        }

        public void Dispose()
        {
            _rows.Clear();
        }

        /// <summary>
        /// 데이터 행 래퍼 클래스
        /// </summary>
        public class DataRow
        {
            private readonly Dictionary<string, string> _data;

            public DataRow(Dictionary<string, string> data)
            {
                _data = data;
            }

            public string GetValue(string columnName)
            {
                if (_data.TryGetValue(columnName, out var value))
                    return value;
                
                // 대소문자 구분 없이 다시 시도
                var key = _data.Keys.FirstOrDefault(k => 
                    k.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                
                return key != null ? _data[key] : string.Empty;
            }

            public bool HasColumn(string columnName)
            {
                return _data.ContainsKey(columnName) ||
                       _data.Keys.Any(k => k.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
