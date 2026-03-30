using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace ExcelConverter.Editor
{
    /// <summary>
    /// Flower 이미지를 Addressable에 자동 등록하고 Plant 데이터와 매핑
    /// </summary>
    public class AddressableFlowerSetup : EditorWindow
    {
        private string _imageFolder = "Assets/Images/Flower/";
        private string _excelFolder = "Assets/ExcelFiles/";
        
        // PlantID → 이미지 파일명 매핑
        private readonly Dictionary<string, string> _plantToImageMap = new()
        {
            { "P001", "high-res rose" },
            { "P002", "high-res sunflower" },
            { "P003", "high-res tulip" },
            { "P004", "high-res cherry blossom" },
            { "P005", "high-res lily of the valley" }  // 토마토 → 은방울꽃 변경
        };

        [MenuItem("Tools/Excel Converter/Setup Flower Images")]
        public static void ShowWindow()
        {
            GetWindow<AddressableFlowerSetup>("Setup Flower Images");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Flower 이미지 Addressable 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            _imageFolder = EditorGUILayout.TextField("이미지 폴더", _imageFolder);
            _excelFolder = EditorGUILayout.TextField("Excel 폴더", _excelFolder);

            EditorGUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("자동 설정 실행", GUILayout.Height(40)))
            {
                SetupAll();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Addressable 키만 등록"))
            {
                RegisterImagesToAddressable();
            }

            if (GUILayout.Button("Excel 파일만 업데이트"))
            {
                UpdateExcelWithAddressableKeys();
            }
        }

        private void SetupAll()
        {
            RegisterImagesToAddressable();
            UpdateExcelWithAddressableKeys();
            
            EditorUtility.DisplayDialog("완료", 
                "모든 설정이 완료되었습니다!\n\n" +
                "1. 이미지가 Addressable에 등록됨\n" +
                "2. Excel 파일에 AddressableKey 컬럼 추가됨\n" +
                "3. PlantData 클래스에 필드 추가됨", 
                "확인");
        }

        /// <summary>
        /// 이미지 파일들을 Addressable에 등록
        /// </summary>
        private void RegisterImagesToAddressable()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings를 찾을 수 없습니다.");
                return;
            }

            var group = settings.FindGroup("Default Local Group");
            if (group == null)
            {
                group = settings.CreateGroup("Default Local Group", false, false, true, null);
            }

            int registeredCount = 0;

            foreach (var mapping in _plantToImageMap)
            {
                var plantID = mapping.Key;
                var imageFileName = mapping.Value;
                var imagePath = Path.Combine(_imageFolder, imageFileName + ".png");
                
                if (!File.Exists(imagePath))
                {
                    Debug.LogWarning($"이미지를 찾을 수 없습니다: {imagePath}");
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(imagePath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"GUID를 찾을 수 없습니다: {imagePath}");
                    continue;
                }

                // 이미 등록되어 있는지 확인
                var existingEntry = settings.FindAssetEntry(guid);
                if (existingEntry != null)
                {
                    Debug.Log($"이미 등록됨: {imageFileName} -> {existingEntry.address}");
                    continue;
                }

                // Addressable 키 생성 (간결하게)
                var addressableKey = GetSimpleKey(imageFileName);

                // Addressable에 등록
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = addressableKey;

                Debug.Log($"등록 완료: {imageFileName} -> {addressableKey}");
                registeredCount++;
            }

            // 변경사항 저장
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"총 {registeredCount}개 이미지가 Addressable에 등록되었습니다.");
        }

        /// <summary>
        /// 간결한 Addressable 키 생성
        /// </summary>
        private string GetSimpleKey(string fileName)
        {
            // "high-res rose" -> "rose"
            // "high-res lily of the valley" -> "lily_of_the_valley"
            var key = fileName.Replace("high-res ", "").Replace(" ", "_").ToLower();
            return key;
        }

        /// <summary>
        /// Excel 파일에 AddressableKey와 SpritePath 컬럼 추가
        /// </summary>
        private void UpdateExcelWithAddressableKeys()
        {
            var csvPath = Path.Combine(_excelFolder, "Plants_Template.csv");
            
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"Plants_Template.csv를 찾을 수 없습니다: {csvPath}");
                return;
            }

            // 새로운 CSV 내용 생성
            var lines = new List<string>();
            
            // 헤더 (AddressableKey, SpritePath 추가)
            lines.Add("PlantID,PlantName,AddressableKey,SpritePath,Category,GrowthTimeMin,SeedPrice,SellPrice_1Star,SellPrice_2Star,SellPrice_3Star,Exp_1Star,Exp_2Star,Exp_3Star,SeasonBonus_Spring,SeasonBonus_Summer,SeasonBonus_Autumn,SeasonBonus_Winter,WeatherBonus_Sunny,WeatherBonus_Rain,WeatherBonus_Cloudy,WeatherBonus_Snow,UnlockLevel,SpecialEffect");

            // 데이터 (P005는 lily of the valley로 변경)
            lines.Add("P001,장미,rose,Images/Flower/high-res rose,Flower,30,50,100,150,200,10,15,20,1.2,1.0,1.1,0.8,1.0,1.2,1.0,0.9,1,");
            lines.Add("P002,해바라기,sunflower,Images/Flower/high-res sunflower,Flower,45,30,80,120,160,15,22,30,1.0,1.3,1.0,0.8,1.2,0.8,1.0,0.9,2,");
            lines.Add("P003,튤립,tulip,Images/Flower/high-res tulip,Flower,25,40,90,135,180,12,18,24,1.3,0.9,1.0,0.9,1.0,1.0,1.0,1.0,1,");
            lines.Add("P004,벚꽃,cherry_blossom,Images/Flower/high-res cherry blossom,Tree,60,100,200,300,400,25,37,50,1.2,1.0,0.8,0.7,1.0,0.9,1.0,0.8,3,");
            lines.Add("P005,은방울꽃,lily_of_the_valley,Images/Flower/high-res lily of the valley,Flower,35,45,85,127,170,13,19,26,1.1,0.9,1.2,0.8,1.0,1.1,0.9,0.8,2,");

            // UTF-8 BOM으로 저장
            File.WriteAllLines(csvPath, lines, new System.Text.UTF8Encoding(true));

            Debug.Log($"Plants_Template.csv 업데이트 완료!");
            Debug.Log("- AddressableKey 컬럼 추가");
            Debug.Log("- SpritePath 컬럼 추가");
            Debug.Log("- P005: 토마토 -> 은방울꽃으로 변경");

            // PlantData.cs 업데이트
            UpdatePlantDataClass();
        }

        /// <summary>
        /// PlantData 클래스에 새 필드 추가
        /// </summary>
        private void UpdatePlantDataClass()
        {
            var plantDataPath = "Assets/Scripts/Data/PlantData.cs";
            
            if (!File.Exists(plantDataPath))
            {
                Debug.LogWarning($"PlantData.cs를 찾을 수 없습니다: {plantDataPath}");
                return;
            }

            var content = File.ReadAllText(plantDataPath);

            // 이미 필드가 있는지 확인
            if (content.Contains("public string AddressableKey"))
            {
                Debug.Log("PlantData.cs에 이미 AddressableKey 필드가 존재합니다.");
                return;
            }

            // SpecialEffect 필드 뒤에 새 필드 추가
            var oldField = "public string SpecialEffect;";
            var newFields = @"public string SpecialEffect;

        [ExcelIgnore]
        public string AddressableKey;

        [ExcelIgnore]
        public string SpritePath;";

            content = content.Replace(oldField, newFields);

            File.WriteAllText(plantDataPath, content);

            Debug.Log("PlantData.cs 업데이트 완료!");
            Debug.Log("- AddressableKey 필드 추가");
            Debug.Log("- SpritePath 필드 추가");
        }
    }
}
