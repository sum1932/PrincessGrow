using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelConverter.Attributes;
using UnityEngine;

namespace ExcelConverter.Core
{
    /// <summary>
    /// Excel 데이터를 C# 객체로 변환하는 메인 컨버터
    /// </summary>
    public class ExcelDataConverter
    {
        private readonly TypeConverter _typeConverter = new();

        public List<T> ParseExcel<T>(string excelPath, string sheetName = null) where T : class, new()
        {
            var results = new List<T>();
            var type = typeof(T);

            var idField = GetIdField(type);
            if (idField == null)
            {
                Debug.LogError($"[{type.Name}] ExcelIdAttribute가 지정된 필드가 없습니다.");
                return results;
            }

            try
            {
                using var reader = new ExcelSheetReader(excelPath, sheetName);
                
                if (reader.Headers.Length == 0)
                {
                    Debug.LogWarning($"[{type.Name}] Excel 파일에 데이터가 없습니다: {excelPath}");
                    return results;
                }

                var fieldMappings = CreateFieldMappings(type, reader.Headers, idField);

                foreach (var row in reader.GetDataRows())
                {
                    T instance;
                    
                    // ScriptableObject인 경우 CreateInstance 사용
                    if (typeof(T).IsSubclassOf(typeof(ScriptableObject)))
                    {
                        instance = ScriptableObject.CreateInstance(typeof(T)) as T;
                    }
                    else
                    {
                        instance = new T();
                    }
                    
                    foreach (var mapping in fieldMappings)
                    {
                        var cellValue = row.GetValue(mapping.ColumnName);
                        var convertedValue = _typeConverter.Convert(cellValue, mapping.FieldType);
                        mapping.FieldInfo.SetValue(instance, convertedValue);
                    }

                    var idValue = idField.GetValue(instance);
                    if (idValue != null && !string.IsNullOrEmpty(idValue.ToString()))
                    {
                        results.Add(instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{type.Name}] Excel 파싱 실패: {ex.Message}");
            }

            Debug.Log($"[{type.Name}] 파싱 완료: {results.Count}개 항목");
            return results;
        }

        public void ConvertAndUpdate<T>(ScriptableObject gameData, string excelPath, string sheetName = null) 
            where T : class, new()
        {
            if (gameData == null)
            {
                Debug.LogError($"[{typeof(T).Name}] GameData가 null입니다.");
                return;
            }

            var newData = ParseExcel<T>(excelPath, sheetName);

            var listField = FindListField<T>(gameData.GetType());
            if (listField == null)
            {
                Debug.LogError($"[{typeof(T).Name}] GameData에 List<{typeof(T).Name}> 필드가 없습니다.");
                return;
            }

            var existingList = listField.GetValue(gameData) as List<T>;
            if (existingList == null)
            {
                existingList = new List<T>();
                listField.SetValue(gameData, existingList);
            }

            UpdateListById(existingList, newData);

            Debug.Log($"[{typeof(T).Name}] 업데이트 완료: 총 {existingList.Count}개 항목");
        }

        private void UpdateListById<T>(List<T> targetList, List<T> newData) where T : class
        {
            var type = typeof(T);
            var idField = GetIdField(type);
            if (idField == null) return;

            var existingMap = new Dictionary<string, T>();
            foreach (var item in targetList)
            {
                var id = idField.GetValue(item)?.ToString();
                if (!string.IsNullOrEmpty(id))
                {
                    existingMap[id] = item;
                }
            }

            var updatedCount = 0;
            var addedCount = 0;

            foreach (var newItem in newData)
            {
                var id = idField.GetValue(newItem)?.ToString();
                if (string.IsNullOrEmpty(id)) continue;

                if (existingMap.TryGetValue(id, out var existing))
                {
                    CopyFields(newItem, existing, type, idField);
                    updatedCount++;
                }
                else
                {
                    targetList.Add(newItem);
                    addedCount++;
                }
            }

            Debug.Log($"업데이트: {updatedCount}개, 추가: {addedCount}개");
        }

        private FieldInfo GetIdField(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(f => f.GetCustomAttribute<ExcelIdAttribute>() != null);
        }

        private FieldInfo FindListField<T>(Type gameDataType)
        {
            return gameDataType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(f => 
                {
                    if (!f.FieldType.IsGenericType) return false;
                    if (f.FieldType.GetGenericTypeDefinition() != typeof(List<>)) return false;
                    return f.FieldType.GetGenericArguments()[0] == typeof(T);
                });
        }

        private List<FieldMapping> CreateFieldMappings(Type type, string[] excelHeaders, FieldInfo idField)
        {
            var mappings = new List<FieldMapping>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<ExcelIgnoreAttribute>() == null);

            foreach (var field in fields)
            {
                var columnAttr = field.GetCustomAttribute<ExcelColumnAttribute>();
                var columnName = columnAttr?.Name ?? field.Name;

                if (excelHeaders.Contains(columnName))
                {
                    mappings.Add(new FieldMapping
                    {
                        FieldInfo = field,
                        ColumnName = columnName,
                        FieldType = field.FieldType
                    });
                }
                else if (field == idField)
                {
                    Debug.LogError($"[{type.Name}] ID 필드 '{columnName}'에 해당하는 컬럼을 Excel에서 찾을 수 없습니다.");
                }
            }

            return mappings;
        }

        private void CopyFields<T>(T source, T target, Type type, FieldInfo excludeField) where T : class
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;
                if (field == excludeField) continue;
                
                field.SetValue(target, field.GetValue(source));
            }
        }
    }
}
