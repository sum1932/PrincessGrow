using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace ExcelConverter.Editor
{
    /// <summary>
    /// 작물 이미지를 Addressable에 자동 등록하고 Plant 데이터와 매핑
    /// Select_Plant_Images 폴더의 이미지들을 자동으로 스캔하여 등록
    /// </summary>
    public class AddressablePlantSetup : EditorWindow
    {
        private string _imageFolder = "Assets/Images/Select_Plant_Images/";
        private string _excelFolder = "Assets/ExcelFiles/";
        private string _csvFileName = "Plants.csv";
        
        // 작물별 AddressableKey 매핑 (폴더명 → AddressableKey)
        private readonly Dictionary<string, string> _folderToKeyMap = new()
        {
            { "Asparagus", "asparagus" },
            { "Corn", "corn" },
            { "RedCobbage", "redcabbage" },
            { "RedPepper", "redpepper" },
            { "Sunflower", "sunflower" },
            { "Zucchini", "zucchini" }
        };

        [MenuItem("Tools/Excel Converter/Setup Plant Images")]
        public static void ShowWindow()
        {
            GetWindow<AddressablePlantSetup>("Setup Plant Images");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("작물 이미지 Addressable 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            _imageFolder = EditorGUILayout.TextField("이미지 폴더", _imageFolder);
            _excelFolder = EditorGUILayout.TextField("Excel 폴더", _excelFolder);
            _csvFileName = EditorGUILayout.TextField("CSV 파일명", _csvFileName);

            EditorGUILayout.Space(20);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("자동 설정 실행 (전체)", GUILayout.Height(40)))
            {
                SetupAll();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("1. Addressable 키만 등록", GUILayout.Height(30)))
            {
                RegisterImagesToAddressable();
            }

            if (GUILayout.Button("2. CSV 파일 업데이트", GUILayout.Height(30)))
            {
                UpdateCsvWithAddressableKeys();
            }
            
            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.2f);
            if (GUILayout.Button("스캔된 이미지 목록 확인", GUILayout.Height(30)))
            {
                ScanAndShowImages();
            }
            GUI.backgroundColor = Color.white;
        }

        private void SetupAll()
        {
            RegisterImagesToAddressable();
            UpdateCsvWithAddressableKeys();
            
            EditorUtility.DisplayDialog("완료", 
                "모든 설정이 완료되었습니다!\n\n" +
                "1. 작물 이미지가 Addressable에 등록됨\n" +
                "2. CSV 파일이 업데이트됨\n\n" +
                "생성된 키 예시:\n" +
                "- asparagus_Stage1\n" +
                "- corn_Stage2_Dry\n" +
                "- sunflower_Stage4", 
                "확인");
        }

        /// <summary>
        /// 이미지 파일들을 스캔하고 Addressable에 등록
        /// </summary>
        private void RegisterImagesToAddressable()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings를 찾을 수 없습니다. Window → Asset Management → Addressables → Groups에서 설정을 생성해주세요.");
                return;
            }

            var group = settings.FindGroup("Default Local Group");
            if (group == null)
            {
                group = settings.CreateGroup("Default Local Group", false, false, true, null);
            }

            int registeredCount = 0;
            int skippedCount = 0;
            var scannedImages = ScanPlantImages();

            foreach (var imageInfo in scannedImages)
            {
                var imagePath = imageInfo.FilePath;
                var addressableKey = imageInfo.AddressableKey;
                
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
                    Debug.Log($"이미 등록됨: {imageInfo.FileName} -> {existingEntry.address}");
                    skippedCount++;
                    continue;
                }

                // Addressable에 등록
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = addressableKey;

                Debug.Log($"등록 완료: {imageInfo.FileName} -> {addressableKey}");
                registeredCount++;
            }

            // 변경사항 저장
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>Addressable 등록 완료!</color>");
            Debug.Log($"- 신규 등록: {registeredCount}개");
            Debug.Log($"- 이미 등록됨: {skippedCount}개");
            Debug.Log($"- 총 스캔: {scannedImages.Count}개");
        }

        /// <summary>
        /// Select_Plant_Images 폴더를 스캔하여 이미지 정보 수집
        /// </summary>
        private List<PlantImageInfo> ScanPlantImages()
        {
            var images = new List<PlantImageInfo>();
            
            if (!Directory.Exists(_imageFolder))
            {
                Debug.LogError($"이미지 폴더를 찾을 수 없습니다: {_imageFolder}");
                return images;
            }

            // 각 작물 폴더 스캔
            foreach (var folderEntry in _folderToKeyMap)
            {
                var folderName = folderEntry.Key;
                var baseKey = folderEntry.Value;
                var plantFolder = Path.Combine(_imageFolder, folderName);
                
                if (!Directory.Exists(plantFolder))
                {
                    Debug.LogWarning($"작물 폴더를 찾을 수 없습니다: {plantFolder}");
                    continue;
                }

                // PNG 파일들 스캔
                var pngFiles = Directory.GetFiles(plantFolder, "*.png", SearchOption.TopDirectoryOnly);
                
                foreach (var pngFile in pngFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(pngFile);
                    var addressableKey = ParseToAddressableKey(fileName, baseKey);
                    
                    if (!string.IsNullOrEmpty(addressableKey))
                    {
                        images.Add(new PlantImageInfo
                        {
                            FilePath = pngFile.Replace("\\", "/"),
                            FileName = Path.GetFileName(pngFile),
                            AddressableKey = addressableKey,
                            PlantKey = baseKey
                        });
                    }
                }
            }

            return images;
        }

        /// <summary>
        /// 파일명을 Addressable 키로 변환
        /// 예: "high-res corn crop stage 1.png" -> "corn_Stage1"
        /// 예: "high-res corn crop (dry) stage 2.png" -> "corn_Stage2_Dry"
        /// </summary>
        private string ParseToAddressableKey(string fileName, string baseKey)
        {
            // crop stage 패턴 체크 (item, seeds 제외)
            if (!fileName.Contains("crop stage"))
            {
                return null; // crop 이미지가 아님 (item, seeds 등)
            }

            // Stage 번호 추출
            var stageMatch = Regex.Match(fileName, @"stage\s+(\d+)", RegexOptions.IgnoreCase);
            if (!stageMatch.Success)
            {
                return null;
            }

            int stageNumber = int.Parse(stageMatch.Groups[1].Value);
            
            // Dry 상태 체크
            bool isDry = fileName.Contains("(dry)");
            
            // 키 생성
            if (isDry)
            {
                return $"{baseKey}_Stage{stageNumber}_Dry";
            }
            else
            {
                return $"{baseKey}_Stage{stageNumber}";
            }
        }

        /// <summary>
        /// CSV 파일 업데이트
        /// </summary>
        private void UpdateCsvWithAddressableKeys()
        {
            var csvPath = Path.Combine(_excelFolder, _csvFileName);
            
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvPath}");
                return;
            }

            // 기존 CSV 읽기
            var lines = File.ReadAllLines(csvPath).ToList();
            if (lines.Count == 0)
            {
                Debug.LogError("CSV 파일이 비어있습니다.");
                return;
            }

            // 헤더 확인 및 수정
            var headers = lines[0].Split(',');
            bool hasAddressableKey = headers.Contains("AddressableKey");
            bool hasSeedAddressableKey = headers.Contains("SeedAddressableKey");
            bool hasSpritePath = headers.Contains("SpritePath");

            if (!hasAddressableKey || !hasSeedAddressableKey || !hasSpritePath)
            {
                // 헤더에 컬럼 추가
                var headerList = headers.ToList();
                if (!hasAddressableKey) headerList.Insert(2, "AddressableKey");
                if (!hasSeedAddressableKey) headerList.Insert(3, "SeedAddressableKey");
                if (!hasSpritePath) headerList.Insert(4, "SpritePath");
                lines[0] = string.Join(",", headerList);
                Debug.Log("CSV 헤더에 AddressableKey, SeedAddressableKey, SpritePath 컬럼 추가");
            }

            // 데이터 라인 업데이트
            for (int i = 1; i < lines.Count; i++)
            {
                var values = lines[i].Split(',');
                var plantId = values[0];
                var plantName = values[1];
                
                // PlantID에 따른 AddressableKey 결정
                string addressableKey = GetAddressableKeyByPlantId(plantId);
                string seedAddressableKey = GetSeedAddressableKeyByPlantId(plantId);
                string spritePath = GetSpritePathByPlantId(plantId);
                
                // 값 업데이트
                var valueList = values.ToList();
                
                // AddressableKey 위치 찾기/설정
                int addrIndex = GetColumnIndex(lines[0], "AddressableKey");
                if (addrIndex >= 0)
                {
                    while (valueList.Count <= addrIndex) valueList.Add("");
                    valueList[addrIndex] = addressableKey;
                }

                // SeedAddressableKey 위치 찾기/설정
                int seedAddrIndex = GetColumnIndex(lines[0], "SeedAddressableKey");
                if (seedAddrIndex >= 0)
                {
                    while (valueList.Count <= seedAddrIndex) valueList.Add("");
                    valueList[seedAddrIndex] = seedAddressableKey;
                }
                
                // SpritePath 위치 찾기/설정
                int spriteIndex = GetColumnIndex(lines[0], "SpritePath");
                if (spriteIndex >= 0)
                {
                    while (valueList.Count <= spriteIndex) valueList.Add("");
                    valueList[spriteIndex] = spritePath;
                }
                
                lines[i] = string.Join(",", valueList);
            }

            // UTF-8 BOM으로 저장
            File.WriteAllLines(csvPath, lines, new System.Text.UTF8Encoding(true));

            Debug.Log($"<color=green>CSV 파일 업데이트 완료!</color>");
            Debug.Log($"- 파일: {csvPath}");
            Debug.Log($"- 라인 수: {lines.Count}");
        }

        /// <summary>
        /// PlantID에 따른 AddressableKey 반환
        /// 작물 기본 키 (예: corn, asparagus)
        /// </summary>
        private string GetAddressableKeyByPlantId(string plantId)
        {
            return plantId switch
            {
                "P001" => "asparagus",
                "P002" => "corn",
                "P003" => "redcabbage",
                "P004" => "redpepper",
                "P005" => "sunflower",
                "P006" => "zucchini",
                _ => ""
            };
        }

        /// <summary>
        /// PlantID에 따른 SeedAddressableKey 반환
        /// 씨앗 이미지용 키 (예: corn_Seeds, asparagus_Seeds)
        /// </summary>
        private string GetSeedAddressableKeyByPlantId(string plantId)
        {
            return plantId switch
            {
                "P001" => "asparagus_Seeds",
                "P002" => "corn_Seeds",
                "P003" => "redcabbage_Seeds",
                "P004" => "redpepper_Seeds",
                "P005" => "sunflower_Seeds",
                "P006" => "zucchini_Seeds",
                _ => ""
            };
        }

        /// <summary>
        /// PlantID에 따른 SpritePath 반환
        /// </summary>
        private string GetSpritePathByPlantId(string plantId)
        {
            return plantId switch
            {
                "P001" => "Images/Select_Plant_Images/Asparagus",
                "P002" => "Images/Select_Plant_Images/Corn",
                "P003" => "Images/Select_Plant_Images/RedCobbage",
                "P004" => "Images/Select_Plant_Images/RedPepper",
                "P005" => "Images/Select_Plant_Images/Sunflower",
                "P006" => "Images/Select_Plant_Images/Zucchini",
                _ => ""
            };
        }

        /// <summary>
        /// CSV 헤더에서 컬럼 인덱스 찾기
        /// </summary>
        private int GetColumnIndex(string headerLine, string columnName)
        {
            var headers = headerLine.Split(',');
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Trim() == columnName)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 스캔된 이미지 목록을 콘솔에 출력
        /// </summary>
        private void ScanAndShowImages()
        {
            var images = ScanPlantImages();
            
            Debug.Log($"<color=cyan>=== 스캔된 작물 이미지 목록 ({images.Count}개) ===</color>");
            
            var grouped = images.GroupBy(img => img.PlantKey);
            foreach (var group in grouped)
            {
                Debug.Log($"<color=yellow>[{group.Key}]</color>");
                foreach (var img in group.OrderBy(i => i.AddressableKey))
                {
                    Debug.Log($"  - {img.FileName} → <color=green>{img.AddressableKey}</color>");
                }
            }
            
            EditorUtility.DisplayDialog("스캔 완료", 
                $"총 {images.Count}개의 이미지를 찾았습니다.\n\n자세한 내용은 Console 창을 확인해주세요.", 
                "확인");
        }

        /// <summary>
        /// 작물 이미지 정보 클래스
        /// </summary>
        private class PlantImageInfo
        {
            public string FilePath;
            public string FileName;
            public string AddressableKey;
            public string PlantKey;
        }
    }
}
