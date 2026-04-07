using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelConverter.Attributes;
using ExcelConverter.Core;
using GameData.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Editor
{
    public class CsvConverterWindow : EditorWindow
    {
        [System.Serializable]
        public class CsvConvertSetting
        {
            public bool Enabled = true;
            public string DatabaseTypeName;  // CharacterDatabase, EventDatabase 등
            public string DataTypeName;      // CharacterData, EventData 등
            public string CsvFileName;
            public string OutputPath;        // Assets/GameData/CharacterDatabase.asset
        }

        private string _csvFolderPath = "C:/우수민/Maker/GameProject/01_GameDesign/Data/";
        private string _outputFolderPath = "Assets/GameData/";
        private List<CsvConvertSetting> _settings = new();
        private Vector2 _scrollPosition;
        private bool _showSettings = true;

        // Database 타입과 데이터 타입 매핑
        private readonly List<(string DatabaseType, string DataType, string CsvFile)> _defaultSettings = new()
        {
            ("CharacterDatabase", "CharacterData", "Characters.csv"),
            ("StatDatabase", "StatData", "Character_Stats.csv"),
            ("EventDatabase", "EventData", "Events.csv"),
            ("DialogueDatabase", "DialogueData", "Dialogues.csv"),
            ("QuestDatabase", "QuestData", "Quests.csv"),
            ("ItemDatabase", "ItemData", "Items_Master.csv"),
            ("EquipmentDatabase", "EquipmentData", "Equipment.csv"),
            ("LocationDatabase", "LocationData", "Locations.csv"),
            ("ActionDatabase", "ActionData", "Actions.csv"),
            ("EndingDatabase", "EndingData", "Ending_Conditions.csv"),
            ("NPCFavorDatabase", "NPCFavorData", "NPC_Favor.csv"),
            ("NPCDialogueDatabase", "NPCDialogueData", "NPC_Dialogues_KO.csv")
        };

        [MenuItem("Tools/CSV Converter")]
        public static void ShowWindow()
        {
            var window = GetWindow<CsvConverterWindow>("CSV Converter");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            LoadDefaultSettings();
        }

        private void LoadDefaultSettings()
        {
            _settings.Clear();
            foreach (var (dbType, dataType, csvFile) in _defaultSettings)
            {
                _settings.Add(new CsvConvertSetting
                {
                    Enabled = true,
                    DatabaseTypeName = dbType,
                    DataTypeName = dataType,
                    CsvFileName = csvFile,
                    OutputPath = _outputFolderPath + dbType + ".asset"
                });
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            DrawHeader();
            DrawPathSettings();
            DrawConvertSettings();
            DrawActionButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("CSV to Database Converter", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("CSV 데이터를 Database ScriptableObject로 변환합니다.", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);
        }

        private void DrawPathSettings()
        {
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CSV 폴더:", GUILayout.Width(100));
            _csvFolderPath = EditorGUILayout.TextField(_csvFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFolderPanel("Select CSV Folder", _csvFolderPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _csvFolderPath = path + "/";
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("출력 폴더:", GUILayout.Width(100));
            _outputFolderPath = EditorGUILayout.TextField(_outputFolderPath);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }

        private void DrawConvertSettings()
        {
            _showSettings = EditorGUILayout.Foldout(_showSettings, "변환 설정", true, EditorStyles.foldoutHeader);
            
            if (_showSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("활성화", GUILayout.Width(60));
                EditorGUILayout.LabelField("Database", GUILayout.Width(150));
                EditorGUILayout.LabelField("CSV 파일명", GUILayout.Width(150));
                EditorGUILayout.LabelField("출력 경로", GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
                
                for (int i = 0; i < _settings.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _settings[i].Enabled = EditorGUILayout.Toggle(_settings[i].Enabled, GUILayout.Width(60));
                    _settings[i].DatabaseTypeName = EditorGUILayout.TextField(_settings[i].DatabaseTypeName, GUILayout.Width(150));
                    _settings[i].CsvFileName = EditorGUILayout.TextField(_settings[i].CsvFileName, GUILayout.Width(150));
                    _settings[i].OutputPath = EditorGUILayout.TextField(_settings[i].OutputPath, GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(10);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("설정 초기화", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("설정 초기화", "모든 설정을 기본값으로 되돌리시겠습니까?", "예", "아니오"))
                {
                    LoadDefaultSettings();
                }
            }
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("모두 변환", GUILayout.Height(30)))
            {
                ConvertAll();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("폴더 생성", GUILayout.Height(25)))
            {
                CreateOutputFolders();
            }
            
            if (GUILayout.Button("기존 Database 삭제", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("데이터 삭제", "모든 Database 파일을 삭제하시겠습니까?", "예", "아니오"))
                {
                    DeleteExistingDatabases();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void ConvertAll()
        {
            int successCount = 0;
            int failCount = 0;

            // 출력 폴더 확인
            if (!Directory.Exists(_outputFolderPath))
            {
                Directory.CreateDirectory(_outputFolderPath);
            }

            foreach (var setting in _settings)
            {
                if (!setting.Enabled) continue;

                try
                {
                    ConvertCsvToDatabase(setting);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{setting.DatabaseTypeName}] 변환 실패: {ex.Message}\n{ex.StackTrace}");
                    failCount++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("변환 완료", 
                $"변환 결과:\n성공: {successCount}\n실패: {failCount}", 
                "확인");
        }

        private void ConvertCsvToDatabase(CsvConvertSetting setting)
        {
            string csvPath = Path.Combine(_csvFolderPath, setting.CsvFileName);
            
            Debug.Log($"[{setting.DatabaseTypeName}] CSV 파일 경로: {csvPath}");
            Debug.Log($"[{setting.DatabaseTypeName}] 파일 존재 여부: {File.Exists(csvPath)}");
            
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning($"CSV 파일을 찾을 수 없습니다: {csvPath}");
                return;
            }

            // Database 타입 찾기
            var databaseType = GetTypeByName(setting.DatabaseTypeName);
            if (databaseType == null)
            {
                Debug.LogError($"Database 타입을 찾을 수 없습니다: {setting.DatabaseTypeName}\n" +
                    "클래스가 제대로 컴파일되었는지 확인하세요.");
                return;
            }

            // CSV 파싱하여 데이터 리스트 생성
            var dataList = ParseCsvToList(csvPath, setting.DataTypeName);
            if (dataList == null)
            {
                Debug.LogError($"CSV 파싱 실패: {setting.CsvFileName}");
                return;
            }
            
            Debug.Log($"[{setting.DatabaseTypeName}] CSV 파싱 완료: {dataList.Count}개 데이터");

            // Database ScriptableObject 로드 또는 생성
            ScriptableObject databaseAsset = null;
            
            if (File.Exists(setting.OutputPath))
            {
                // 기존 Database 로드
                databaseAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(setting.OutputPath);
                Debug.Log($"[{setting.DatabaseTypeName}] 기존 Database 업데이트: {setting.OutputPath}");
                
                // 기존 Sub-Asset들 삭제
                RemoveExistingSubAssets(databaseAsset);
            }
            else
            {
                // 새 Database 생성
                databaseAsset = ScriptableObject.CreateInstance(databaseType);
                AssetDatabase.CreateAsset(databaseAsset, setting.OutputPath);
                Debug.Log($"[{setting.DatabaseTypeName}] 새 Database 생성: {setting.OutputPath}");
            }

            // 데이터 타입 찾기
            var dataType = GetTypeByName(setting.DataTypeName);
            if (dataType == null)
            {
                Debug.LogError($"데이터 타입을 찾을 수 없습니다: {setting.DataTypeName}");
                return;
            }

            // 새로운 List<T> 생성
            var newListType = typeof(List<>).MakeGenericType(dataType);
            var newList = (System.Collections.IList)Activator.CreateInstance(newListType);
            
            // 각 Data를 Sub-Asset으로 추가
            int index = 0;
            foreach (var item in dataList)
            {
                if (item is ScriptableObject so)
                {
                    so.name = $"{setting.DataTypeName}_{index}";
                    AssetDatabase.AddObjectToAsset(so, databaseAsset);
                    newList.Add(so);
                    
                    // DialogueData인 경우 JSON 파싱 수행
                    if (so is GameData.ScriptableObjects.DialogueData dialogueData)
                    {
                        dialogueData.ParseJson();
                    }
                    
                    index++;
                }
            }
            
            // _data 필드에 새 리스트 할당
            var dataField = databaseType.GetField("_data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dataField == null)
            {
                Debug.LogError($"{setting.DatabaseTypeName}에 _data 필드를 찾을 수 없습니다.");
                return;
            }
            
            dataField.SetValue(databaseAsset, newList);
            
            Debug.Log($"[{setting.DatabaseTypeName}] _data 필드에 {newList.Count}개의 데이터(Sub-Asset) 설정 완료");

            EditorUtility.SetDirty(databaseAsset);
            AssetDatabase.SaveAssets();

            Debug.Log($"[{setting.DatabaseTypeName}] {dataList.Count}개의 데이터 변환 완료");
        }

        private void RemoveExistingSubAssets(ScriptableObject databaseAsset)
        {
            var path = AssetDatabase.GetAssetPath(databaseAsset);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            
            foreach (var asset in assets)
            {
                // Database 자체는 제외
                if (asset == databaseAsset) continue;
                
                // Sub-Asset 삭제
                AssetDatabase.RemoveObjectFromAsset(asset);
                UnityEngine.Object.DestroyImmediate(asset);
            }
            
            Debug.Log($"[{databaseAsset.GetType().Name}] 기존 Sub-Asset {assets.Length - 1}개 삭제 완료");
        }

        private System.Collections.IList ParseCsvToList(string csvPath, string dataTypeName)
        {
            var dataType = GetTypeByName(dataTypeName);
            if (dataType == null)
            {
                Debug.LogError($"데이터 타입을 찾을 수 없습니다: {dataTypeName}");
                return null;
            }

            // List<T> 타입 생성
            var listType = typeof(List<>).MakeGenericType(dataType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType);

            Debug.Log($"[ParseCsvToList] CSV 읽기 시작: {csvPath}");
            
            using var reader = new CsvSheetReader(csvPath);
            Debug.Log($"[ParseCsvToList] Headers: {string.Join(", ", reader.Headers)}");
            
            if (reader.Headers.Length == 0)
            {
                Debug.LogWarning($"CSV 파일이 비어있습니다: {csvPath}");
                return list;
            }

            var fieldMappings = CreateFieldMappings(dataType, reader.Headers);
            Debug.Log($"[ParseCsvToList] 필드 매핑 수: {fieldMappings.Count}");

            int rowCount = 0;
            foreach (var row in reader.GetDataRows())
            {
                // ScriptableObject는 CreateInstance로 생성
                object instance;
                if (typeof(ScriptableObject).IsAssignableFrom(dataType))
                {
                    instance = ScriptableObject.CreateInstance(dataType);
                }
                else
                {
                    instance = Activator.CreateInstance(dataType);
                }
                
                foreach (var mapping in fieldMappings)
                {
                    var cellValue = row.GetValue(mapping.ColumnName);
                    var convertedValue = ConvertValue(cellValue, mapping.FieldType);
                    mapping.FieldInfo.SetValue(instance, convertedValue);
                }

                list.Add(instance);
                rowCount++;
            }
            
            Debug.Log($"[ParseCsvToList] CSV 파싱 완료: {rowCount}개 행, {list.Count}개 데이터");

            return list;
        }

        private List<FieldMapping> CreateFieldMappings(Type type, string[] headers)
        {
            var mappings = new List<FieldMapping>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var columnAttr = field.GetCustomAttribute<ExcelColumnAttribute>();
                if (columnAttr == null) continue;

                var header = headers.FirstOrDefault(h => 
                    h.Equals(columnAttr.Name, StringComparison.OrdinalIgnoreCase));
                
                if (header != null)
                {
                    mappings.Add(new FieldMapping
                    {
                        FieldInfo = field,
                        ColumnName = columnAttr.Name,
                        FieldType = field.FieldType
                    });
                }
            }

            return mappings;
        }

        private object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (targetType == typeof(string))
                    return string.Empty;
                if (targetType == typeof(bool))
                    return false;
                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);
                return null;
            }

            try
            {
                if (targetType == typeof(bool))
                {
                    var upperValue = value.ToUpper();
                    return upperValue == "TRUE" || upperValue == "1" || upperValue == "YES";
                }

                if (targetType == typeof(int))
                {
                    if (int.TryParse(value, out var intResult))
                        return intResult;
                    return 0;
                }

                if (targetType == typeof(float))
                {
                    if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var floatResult))
                        return floatResult;
                    return 0f;
                }

                if (targetType == typeof(string))
                {
                    return value;
                }

                return System.Convert.ChangeType(value, targetType);
            }
            catch
            {
                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);
                return null;
            }
        }

        private Type GetTypeByName(string typeName)
        {
            // 우선 GameData.ScriptableObjects 네임스페이스에서 찾기
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("UnityEditor")) continue;
                if (assembly.FullName.StartsWith("ExcelConverter.Editor")) continue;
                
                try
                {
                    var type = assembly.GetTypes().FirstOrDefault(t => 
                        t.Name == typeName && 
                        t.Namespace == "GameData.ScriptableObjects");
                    if (type != null) return type;
                }
                catch
                {
                    continue;
                }
            }
            
            // 없으면 다른 네임스페이스에서 찾기 (단, System 네임스페이스 제외)
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("UnityEditor")) continue;
                if (assembly.FullName.StartsWith("ExcelConverter.Editor")) continue;
                if (assembly.FullName.StartsWith("System")) continue;
                
                try
                {
                    var type = assembly.GetTypes().FirstOrDefault(t => 
                        t.Name == typeName && 
                        !t.Namespace.StartsWith("System") &&
                        !t.Namespace.StartsWith("Microsoft"));
                    if (type != null) return type;
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        private void CreateOutputFolders()
        {
            if (!Directory.Exists(_outputFolderPath))
            {
                Directory.CreateDirectory(_outputFolderPath);
                Debug.Log($"폴더 생성: {_outputFolderPath}");
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "출력 폴더가 생성되었습니다.", "확인");
        }

        private void DeleteExistingDatabases()
        {
            if (Directory.Exists(_outputFolderPath))
            {
                var files = Directory.GetFiles(_outputFolderPath, "*.asset");
                foreach (var file in files)
                {
                    FileUtil.DeleteFileOrDirectory(file);
                }
                AssetDatabase.Refresh();
                Debug.Log("모든 Database 파일 삭제 완료");
            }
        }
    }
}
