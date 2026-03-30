using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExcelConverter.Core;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Editor
{
    public class ExcelConverterWindow : EditorWindow
    {
        [System.Serializable]
        public class ConvertSetting
        {
            public bool Enabled = true;
            public string DataTypeName;
            public string ExcelFileName;
            public string SheetName;
        }

        private ScriptableObject _gameData;
        private string _excelFolderPath = "Assets/ExcelFiles/";
        private string _gameDataPath = "Assets/Prefabs/GameData.asset";
        private List<ConvertSetting> _settings = new();
        private Vector2 _scrollPosition;
        private bool _showSettings = true;
        private bool _showAdvanced = false;

        [MenuItem("Tools/Excel Converter")]
        public static void ShowWindow()
        {
            var window = GetWindow<ExcelConverterWindow>("Excel Converter");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            DrawHeader();
            DrawPathSettings();
            DrawConvertSettings();
            DrawActionButtons();
            DrawStatus();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Excel to ScriptableObject Converter", 
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter });
            EditorGUILayout.Space(10);
        }

        private void DrawPathSettings()
        {
            _showSettings = EditorGUILayout.Foldout(_showSettings, "경로 설정", true);
            if (!_showSettings) return;

            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            _excelFolderPath = EditorGUILayout.TextField("Excel 폴더", _excelFolderPath);
            if (GUILayout.Button("찾기", GUILayout.Width(60)))
            {
                var selected = EditorUtility.OpenFolderPanel("Excel 폴더 선택", _excelFolderPath, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    _excelFolderPath = selected.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _gameDataPath = EditorGUILayout.TextField("GameData 경로", _gameDataPath);
            if (GUILayout.Button("찾기", GUILayout.Width(60)))
            {
                var selected = EditorUtility.OpenFilePanel("GameData 선택", "Assets", "asset");
                if (!string.IsNullOrEmpty(selected))
                {
                    _gameDataPath = selected.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();

            _gameData = EditorGUILayout.ObjectField("GameData", _gameData, typeof(ScriptableObject), false) as ScriptableObject;

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        private void DrawConvertSettings()
        {
            EditorGUILayout.LabelField("변환 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("설정 추가"))
            {
                _settings.Add(new ConvertSetting());
            }
            if (GUILayout.Button("기본 설정 로드"))
            {
                LoadDefaultSettings();
            }
            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
            
            for (int i = 0; i < _settings.Count; i++)
            {
                var setting = _settings[i];
                EditorGUILayout.BeginHorizontal("box");
                
                setting.Enabled = EditorGUILayout.Toggle(setting.Enabled, GUILayout.Width(20));
                
                EditorGUILayout.BeginVertical();
                setting.DataTypeName = EditorGUILayout.TextField("데이터 타입", setting.DataTypeName);
                setting.ExcelFileName = EditorGUILayout.TextField("Excel 파일", setting.ExcelFileName);
                setting.SheetName = EditorGUILayout.TextField("시트명", setting.SheetName);
                EditorGUILayout.EndVertical();
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _settings.RemoveAt(i);
                    i--;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(10);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("전체 변환", GUILayout.Height(35)))
            {
                ConvertAll();
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("GameData 생성", GUILayout.Height(35)))
            {
                CreateGameData();
            }
            
            EditorGUILayout.EndHorizontal();

            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "고급 기능");
            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;
                if (GUILayout.Button("설정 저장"))
                {
                    SaveSettings();
                }
                if (GUILayout.Button("Excel 폴더 열기"))
                {
                    EditorUtility.RevealInFinder(Path.GetFullPath(_excelFolderPath));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
        }

        private void DrawStatus()
        {
            EditorGUILayout.LabelField("상태", EditorStyles.boldLabel);
            
            var excelFolderExists = Directory.Exists(_excelFolderPath);
            EditorGUILayout.LabelField($"Excel 폴더: {(excelFolderExists ? "✓ 존재함" : "✗ 없음")}");
            
            var gameDataExists = _gameData != null || File.Exists(_gameDataPath);
            EditorGUILayout.LabelField($"GameData: {(gameDataExists ? "✓ 존재함" : "✗ 없음")}");
            
            EditorGUILayout.LabelField($"변환 설정: {_settings.Count}개");
        }

        private void ConvertAll()
        {
            var gameData = _gameData;
            if (gameData == null)
            {
                gameData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(_gameDataPath);
                if (gameData == null)
                {
                    EditorUtility.DisplayDialog("오류", "GameData를 찾을 수 없습니다. 먼저 GameData를 생성하세요.", "확인");
                    return;
                }
            }

            var converter = new ExcelDataConverter();
            var convertedCount = 0;

            foreach (var setting in _settings)
            {
                if (!setting.Enabled) continue;
                if (string.IsNullOrEmpty(setting.DataTypeName)) continue;

                try
                {
                    var type = FindType(setting.DataTypeName);
                    if (type == null)
                    {
                        Debug.LogError($"[{setting.DataTypeName}] 타입을 찾을 수 없습니다.");
                        continue;
                    }

                    var excelPath = Path.Combine(_excelFolderPath, setting.ExcelFileName);
                    if (!File.Exists(excelPath))
                    {
                        Debug.LogError($"[{setting.DataTypeName}] Excel 파일을 찾을 수 없습니다: {excelPath}");
                        continue;
                    }

                    var method = typeof(ExcelDataConverter).GetMethod("ConvertAndUpdate")
                        .MakeGenericMethod(type);
                    method.Invoke(converter, new object[] { gameData, excelPath, setting.SheetName });
                    
                    convertedCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{setting.DataTypeName}] 변환 실패: {ex.Message}");
                }
            }

            EditorUtility.SetDirty(gameData);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("완료", $"{convertedCount}개 타입 변환 완료!", "확인");
            Debug.Log($"[ExcelConverter] 총 {convertedCount}개 타입 변환 완료");
        }

        private void CreateGameData()
        {
            var directory = Path.GetDirectoryName(_gameDataPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var gameDataType = FindType("GameData");
            if (gameDataType == null)
            {
                EditorUtility.DisplayDialog("오류", "GameData 클래스를 찾을 수 없습니다. 먼저 GameData 클래스를 생성하세요.", "확인");
                return;
            }

            var instance = ScriptableObject.CreateInstance(gameDataType);
            AssetDatabase.CreateAsset(instance, _gameDataPath);
            AssetDatabase.SaveAssets();

            _gameData = instance;
            EditorUtility.DisplayDialog("완료", "GameData가 생성되었습니다!", "확인");
        }

        private Type FindType(string typeName)
        {
            var type = Assembly.GetExecutingAssembly().GetType(typeName);
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;

                foreach (var t in assembly.GetTypes())
                {
                    if (t.Name == typeName)
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        private void LoadDefaultSettings()
        {
            _settings = new List<ConvertSetting>
            {
                new ConvertSetting { DataTypeName = "PlantData", ExcelFileName = "Plants.xlsx", SheetName = "Plants" },
                new ConvertSetting { DataTypeName = "ItemData", ExcelFileName = "Items.xlsx", SheetName = "Items" },
                new ConvertSetting { DataTypeName = "AnimalHelperData", ExcelFileName = "AnimalHelpers.xlsx", SheetName = "Helpers" },
                new ConvertSetting { DataTypeName = "GardenLevelData", ExcelFileName = "Levels.xlsx", SheetName = "Levels" },
                new ConvertSetting { DataTypeName = "LevelUnlockReward", ExcelFileName = "LevelUnlocks.xlsx", SheetName = "LevelUnlocks" }
            };
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString("ExcelConverter_ExcelFolder", _excelFolderPath);
            EditorPrefs.SetString("ExcelConverter_GameDataPath", _gameDataPath);
            EditorPrefs.SetInt("ExcelConverter_SettingCount", _settings.Count);
            
            for (int i = 0; i < _settings.Count; i++)
            {
                EditorPrefs.SetString($"ExcelConverter_Setting_{i}_Type", _settings[i].DataTypeName);
                EditorPrefs.SetString($"ExcelConverter_Setting_{i}_File", _settings[i].ExcelFileName);
                EditorPrefs.SetString($"ExcelConverter_Setting_{i}_Sheet", _settings[i].SheetName);
                EditorPrefs.SetBool($"ExcelConverter_Setting_{i}_Enabled", _settings[i].Enabled);
            }
            
            Debug.Log("[ExcelConverter] 설정 저장 완료");
        }

        private void LoadSettings()
        {
            _excelFolderPath = EditorPrefs.GetString("ExcelConverter_ExcelFolder", "Assets/ExcelFiles/");
            _gameDataPath = EditorPrefs.GetString("ExcelConverter_GameDataPath", "Assets/Prefabs/GameData.asset");
            
            var count = EditorPrefs.GetInt("ExcelConverter_SettingCount", 0);
            _settings.Clear();
            
            for (int i = 0; i < count; i++)
            {
                _settings.Add(new ConvertSetting
                {
                    DataTypeName = EditorPrefs.GetString($"ExcelConverter_Setting_{i}_Type", ""),
                    ExcelFileName = EditorPrefs.GetString($"ExcelConverter_Setting_{i}_File", ""),
                    SheetName = EditorPrefs.GetString($"ExcelConverter_Setting_{i}_Sheet", ""),
                    Enabled = EditorPrefs.GetBool($"ExcelConverter_Setting_{i}_Enabled", true)
                });
            }

            if (_settings.Count == 0)
            {
                LoadDefaultSettings();
            }
        }
    }
}
