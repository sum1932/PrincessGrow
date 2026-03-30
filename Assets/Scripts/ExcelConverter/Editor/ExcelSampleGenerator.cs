using System;
using System.IO;
using ExcelDataReader;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Editor
{
    /// <summary>
    /// 샘플 Excel 파일 생성 유틸리티
    /// </summary>
    public class ExcelSampleGenerator : EditorWindow
    {
        private string _outputFolder = "Assets/ExcelFiles/";

        [MenuItem("Tools/Excel Converter/Generate Sample Files")]
        public static void ShowWindow()
        {
            GetWindow<ExcelSampleGenerator>("Excel Sample Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("샘플 Excel 파일 생성", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            _outputFolder = EditorGUILayout.TextField("출력 폴더", _outputFolder);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Plants.xlsx 생성", GUILayout.Height(30)))
            {
                GeneratePlantsExcel();
            }

            if (GUILayout.Button("Items.xlsx 생성", GUILayout.Height(30)))
            {
                GenerateItemsExcel();
            }

            if (GUILayout.Button("AnimalHelpers.xlsx 생성", GUILayout.Height(30)))
            {
                GenerateHelpersExcel();
            }

            if (GUILayout.Button("Levels.xlsx 생성", GUILayout.Height(30)))
            {
                GenerateLevelsExcel();
            }

            if (GUILayout.Button("LevelUnlocks.xlsx 생성", GUILayout.Height(30)))
            {
                GenerateLevelUnlocksExcel();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("모든 샘플 생성", GUILayout.Height(40)))
            {
                GeneratePlantsExcel();
                GenerateItemsExcel();
                GenerateHelpersExcel();
                GenerateLevelsExcel();
                GenerateLevelUnlocksExcel();
            }
        }

        private void GeneratePlantsExcel()
        {
            // Excel 파일은 바이너리 포맷이므로 직접 생성은 복잡합니다
            // 대신 템플릿 파일을 생성하거나 수동 생성 가이드를 제공합니다
            
            var directory = Path.GetFullPath(_outputFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, "Plants.xlsx");
            
            // CSV로 먼저 생성 (Excel에서 열 수 있음)
            var csvPath = Path.Combine(directory, "Plants_Template.csv");
            var csvContent = GeneratePlantsCSV();
            // UTF-8 BOM으로 저장하여 Excel에서 한글 깨짐 방지
            File.WriteAllText(csvPath, csvContent, new System.Text.UTF8Encoding(true));
            
            EditorUtility.DisplayDialog("완료", 
                $"Plants_Template.csv가 생성되었습니다.\n위치: {csvPath}\n\n" +
                "이 파일을 Excel로 열어 'Plants' 시트로 저장하세요.", 
                "확인");
            
            EditorUtility.RevealInFinder(csvPath);
            
            Debug.Log($"[ExcelSampleGenerator] Plants 템플릿 생성: {csvPath}");
        }

        private void GenerateItemsExcel()
        {
            var directory = Path.GetFullPath(_outputFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var csvPath = Path.Combine(directory, "Items_Template.csv");
            var csvContent = GenerateItemsCSV();
            // UTF-8 BOM으로 저장하여 Excel에서 한글 깨짐 방지
            File.WriteAllText(csvPath, csvContent, new System.Text.UTF8Encoding(true));
            
            EditorUtility.DisplayDialog("완료", 
                $"Items_Template.csv가 생성되었습니다.\n위치: {csvPath}", 
                "확인");
            
            EditorUtility.RevealInFinder(csvPath);
        }

        private void GenerateHelpersExcel()
        {
            var directory = Path.GetFullPath(_outputFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var csvPath = Path.Combine(directory, "AnimalHelpers_Template.csv");
            var csvContent = GenerateHelpersCSV();
            // UTF-8 BOM으로 저장하여 Excel에서 한글 깨짐 방지
            File.WriteAllText(csvPath, csvContent, new System.Text.UTF8Encoding(true));
            
            EditorUtility.DisplayDialog("완료", 
                $"AnimalHelpers_Template.csv가 생성되었습니다.\n위치: {csvPath}", 
                "확인");
            
            EditorUtility.RevealInFinder(csvPath);
        }

        private void GenerateLevelsExcel()
        {
            var directory = Path.GetFullPath(_outputFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var csvPath = Path.Combine(directory, "Levels_Template.csv");
            var csvContent = GenerateLevelsCSV();
            // UTF-8 BOM으로 저장하여 Excel에서 한글 깨짐 방지
            File.WriteAllText(csvPath, csvContent, new System.Text.UTF8Encoding(true));
            
            EditorUtility.DisplayDialog("완료", 
                $"Levels_Template.csv가 생성되었습니다.\n위치: {csvPath}", 
                "확인");
            
            EditorUtility.RevealInFinder(csvPath);
        }

        private void GenerateLevelUnlocksExcel()
        {
            var directory = Path.GetFullPath(_outputFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var csvPath = Path.Combine(directory, "LevelUnlocks_Template.csv");
            var csvContent = GenerateLevelUnlocksCSV();
            // UTF-8 BOM으로 저장하여 Excel에서 한글 깨짐 방지
            File.WriteAllText(csvPath, csvContent, new System.Text.UTF8Encoding(true));
            
            EditorUtility.DisplayDialog("완료", 
                $"LevelUnlocks_Template.csv가 생성되었습니다.\n위치: {csvPath}", 
                "확인");
            
            EditorUtility.RevealInFinder(csvPath);
        }

        private string GeneratePlantsCSV()
        {
            var sb = new System.Text.StringBuilder();
            
            // 헤더 (AddressableKey, SpritePath 추가)
            sb.AppendLine("PlantID,PlantName,AddressableKey,SpritePath,Category,GrowthTimeMin,SeedPrice,SellPrice_1Star,SellPrice_2Star,SellPrice_3Star,Exp_1Star,Exp_2Star,Exp_3Star,SeasonBonus_Spring,SeasonBonus_Summer,SeasonBonus_Autumn,SeasonBonus_Winter,WeatherBonus_Sunny,WeatherBonus_Rain,WeatherBonus_Cloudy,WeatherBonus_Snow,UnlockLevel,SpecialEffect");
            
            // 데이터 (P005: 토마토 -> 은방울꽃 변경)
            sb.AppendLine("P001,장미,rose,Images/Flower/high-res rose,Flower,30,50,100,150,200,10,15,20,1.2,1.0,1.1,0.8,1.0,1.2,1.0,0.9,1,");
            sb.AppendLine("P002,해바라기,sunflower,Images/Flower/high-res sunflower,Flower,45,30,80,120,160,15,22,30,1.0,1.3,1.0,0.8,1.2,0.8,1.0,0.9,2,");
            sb.AppendLine("P003,튤립,tulip,Images/Flower/high-res tulip,Flower,25,40,90,135,180,12,18,24,1.3,0.9,1.0,0.9,1.0,1.0,1.0,1.0,1,");
            sb.AppendLine("P004,벚꽃,cherry_blossom,Images/Flower/high-res cherry blossom,Tree,60,100,200,300,400,25,37,50,1.2,1.0,0.8,0.7,1.0,0.9,1.0,0.8,3,");
            sb.AppendLine("P005,은방울꽃,lily_of_the_valley,Images/Flower/high-res lily of the valley,Flower,35,45,85,127,170,13,19,26,1.1,0.9,1.2,0.8,1.0,1.1,0.9,0.8,2,");
            
            return sb.ToString();
        }

        private string GenerateItemsCSV()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("ItemID,ItemName,ItemType,BuyPrice,SellPrice,Effect,DurationMin,StackLimit,Source_Shop,Source_Drop,Source_Craft");
            sb.AppendLine("I001,장미 씨앗,Seed,50,25,,0,99,Shop,Drop,");
            sb.AppendLine("I002,비료,Consumable,100,50,GrowthSpeedUp,30,50,Shop,,Craft");
            sb.AppendLine("I003,물뿌리개,Tool,200,100,AutoWater,0,1,Shop,,");
            sb.AppendLine("I004,코인,Currency,0,0,,0,99999,Shop,Drop,");
            
            return sb.ToString();
        }

        private string GenerateHelpersCSV()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("HelperID,HelperName,Nickname,Role,UnlockLevel,MaxEnergy,EnergyCost,RecoverPer10Min,WorkIntervalMin,StarRateBonus,GreetingLine,FavoritePlant1,FavoritePlant2,RandomLine1,RandomLine2");
            sb.AppendLine("H001,토끼,바니,Water,3,100,10,5,5,0.05,\"안녕! 물 주기 도와줄게!\",P001,P003,\"오늘도 화이팅!\",\"물 주는건 내 전문이야!\"");
            sb.AppendLine("H002,다람쥐,쿠키,Harvest,5,80,15,3,10,0.03,\"안녕하세요! 수확 도와드릴까요?\",P002,P004,\"도토리 맛있어요!\",\"열심히 일할게요!\"");
            
            return sb.ToString();
        }

        private string GenerateLevelsCSV()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("level,requiredExp,cumulativeExp,quality2StarBonus,quality3StarBonus,maxWaterSlots,gardenFloors");
            sb.AppendLine("1,100,0,0,0,1,1");
            sb.AppendLine("2,100,100,0.05,0,1,1");
            sb.AppendLine("3,200,300,0.05,0.02,2,1");
            sb.AppendLine("4,400,700,0.1,0.02,2,1");
            sb.AppendLine("5,800,1500,0.1,0.05,3,1");
            sb.AppendLine("6,1500,3000,0.15,0.05,3,1");
            sb.AppendLine("7,2500,5500,0.15,0.08,4,2");
            sb.AppendLine("8,4000,9500,0.2,0.08,4,2");
            sb.AppendLine("9,6000,15500,0.2,0.1,5,2");
            sb.AppendLine("10,10000,25500,0.25,0.15,5,3");
            
            return sb.ToString();
        }

        private string GenerateLevelUnlocksCSV()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("Id,Level,Type,ItemId,FeatureName,Description");
            sb.AppendLine("UL001,2,Plant,carrot,,당근 씨앗 해금");
            sb.AppendLine("UL002,3,Slot,,slot2,정원 슬롯 2개 추가");
            sb.AppendLine("UL003,4,Plant,tomato,,토마토 씨앗 해금");
            sb.AppendLine("UL004,5,AnimalHelper,rabbit,,토끼 도우미 해금");
            sb.AppendLine("UL005,6,Feature,,fertilizer,비료 기능 해금");
            sb.AppendLine("UL006,7,Floor,,floor2,정원 2층 해금");
            sb.AppendLine("UL007,8,Plant,sunflower,,해바라기 씨앗 해금");
            sb.AppendLine("UL008,8,AnimalHelper,squirrel,,다람쥐 도우미 해금");
            sb.AppendLine("UL009,9,Slot,,slot3,정원 슬롯 3개 추가");
            sb.AppendLine("UL010,10,Feature,,premium,프리미엄 기능 해금");
            
            return sb.ToString();
        }
    }
}
