using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDataReader;
using UnityEngine;

namespace ExcelConverter.Core
{
    /// <summary>
    /// Excel 파일을 읽어 데이터로 변환 (DataSet 의존성 제거)
    /// </summary>
    public class ExcelSheetReader : IDisposable
    {
        private List<string> _headers = new();
        private List<List<object>> _rows = new();
        private Dictionary<string, int> _headerIndexMap = new();

        public string[] Headers => _headers.ToArray();

        public ExcelSheetReader(string excelPath, string sheetName = null)
        {
            if (!File.Exists(excelPath))
            {
                throw new FileNotFoundException($"Excel file not found: {excelPath}");
            }

            try
            {
                using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                
                // 시트 선택
                if (!string.IsNullOrEmpty(sheetName))
                {
                    // 시트명으로 이동
                    bool sheetFound = false;
                    do
                    {
                        if (reader.Name == sheetName)
                        {
                            sheetFound = true;
                            break;
                        }
                    } while (reader.NextResult());
                    
                    if (!sheetFound)
                    {
                        throw new ArgumentException($"Sheet '{sheetName}' not found in {excelPath}");
                    }
                }

                // 헤더 읽기 (첫 번째 행)
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var headerValue = reader.GetValue(i)?.ToString()?.Trim() ?? $"Column{i}";
                        _headers.Add(headerValue);
                        _headerIndexMap[headerValue] = i;
                    }
                }

                // 데이터 행 읽기
                while (reader.Read())
                {
                    var row = new List<object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row.Add(value);
                    }
                    _rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExcelSheetReader] Failed to read Excel file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 데이터 행들을 반환합니다.
        /// </summary>
        public IEnumerable<ExcelRow> GetDataRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                yield return new ExcelRow(_headers, _rows[i], _headerIndexMap);
            }
        }

        /// <summary>
        /// 특정 컬럼의 값을 가져옵니다.
        /// </summary>
        public object GetValue(ExcelRow row, string columnName)
        {
            return row.GetValue(columnName);
        }

        public void Dispose()
        {
            _headers?.Clear();
            _rows?.Clear();
            _headerIndexMap?.Clear();
        }
    }

    /// <summary>
    /// Excel 데이터 행을 나타냅니다.
    /// </summary>
    public class ExcelRow
    {
        private readonly List<string> _headers;
        private readonly List<object> _values;
        private readonly Dictionary<string, int> _headerIndexMap;

        public ExcelRow(List<string> headers, List<object> values, Dictionary<string, int> headerIndexMap)
        {
            _headers = headers;
            _values = values;
            _headerIndexMap = headerIndexMap;
        }

        /// <summary>
        /// 특정 컬럼의 값을 가져옵니다.
        /// </summary>
        public object GetValue(string columnName)
        {
            if (_headerIndexMap.TryGetValue(columnName, out int index))
            {
                if (index < _values.Count)
                {
                    var value = _values[index];
                    return value == DBNull.Value ? null : value;
                }
            }
            return null;
        }

        /// <summary>
        /// 인덱스로 값을 가져옵니다.
        /// </summary>
        public object GetValue(int index)
        {
            if (index >= 0 && index < _values.Count)
            {
                var value = _values[index];
                return value == DBNull.Value ? null : value;
            }
            return null;
        }
    }
}
