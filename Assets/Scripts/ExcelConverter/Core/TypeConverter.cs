using System;
using System.Globalization;
using UnityEngine;

namespace ExcelConverter.Core
{
    /// <summary>
    /// Excel 셀 값을 C# 타입으로 변환하는 유틸리티
    /// </summary>
    public class TypeConverter
    {
        public object Convert(object value, Type targetType)
        {
            if (value == null)
                return GetDefault(targetType);

            var stringValue = value.ToString().Trim();
            if (string.IsNullOrEmpty(stringValue))
                return GetDefault(targetType);

            try
            {
                // Nullable 타입 처리
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                // Enum 처리 (문자열 기반)
                if (underlyingType.IsEnum)
                {
                    var cleanValue = stringValue.Replace(" ", "").Replace("_", "");
                    return Enum.Parse(underlyingType, cleanValue, ignoreCase: true);
                }

                // 기본 타입 변환
                if (underlyingType == typeof(int))
                    return int.Parse(stringValue, CultureInfo.InvariantCulture);
                
                if (underlyingType == typeof(float))
                    return float.Parse(stringValue, CultureInfo.InvariantCulture);
                
                if (underlyingType == typeof(double))
                    return double.Parse(stringValue, CultureInfo.InvariantCulture);
                
                if (underlyingType == typeof(bool))
                    return ParseBool(stringValue);
                
                if (underlyingType == typeof(string))
                    return stringValue;

                return System.Convert.ChangeType(stringValue, underlyingType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TypeConverter] Failed to convert '{stringValue}' to {targetType.Name}: {ex.Message}");
                return GetDefault(targetType);
            }
        }

        private bool ParseBool(string value)
        {
            var lower = value.ToLowerInvariant();
            return lower == "true" || lower == "1" || lower == "yes" || lower == "y";
        }

        private object GetDefault(Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                if (type == typeof(int)) return 0;
                if (type == typeof(float)) return 0f;
                if (type == typeof(double)) return 0.0;
                if (type == typeof(bool)) return false;
                if (type == typeof(long)) return 0L;
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
