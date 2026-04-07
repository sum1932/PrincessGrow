using System.Collections.Generic;
using System.Linq;
using GameData.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Editor
{
    /// <summary>
    /// GameDatabase에 ScriptableObject 데이터를 자동으로 연결하는 Editor Tool
    /// </summary>
    public class GameDatabaseEditor : EditorWindow
    {
        private GameDatabase _gameDatabase;
        private Vector2 _scrollPosition;
        private bool _showDetails = true;

        [MenuItem("Tools/Game Database")]
        public static void ShowWindow()
        {
            GetWindow<GameDatabaseEditor>("Game Database");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            DrawHeader();
            DrawDatabaseSelector();
            
            if (_gameDatabase != null)
            {
                DrawStats();
                DrawActions();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Game Database Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("ScriptableObject 데이터베이스 관리 도구", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);
        }

        private void DrawDatabaseSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Game Database:", GUILayout.Width(100));
            _gameDatabase = (GameDatabase)EditorGUILayout.ObjectField(_gameDatabase, typeof(GameDatabase), false);
            
            if (GUILayout.Button("Create New", GUILayout.Width(100)))
            {
                CreateNewDatabase();
            }
            
            if (GUILayout.Button("Find Existing", GUILayout.Width(100)))
            {
                FindExistingDatabase();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        private void DrawStats()
        {
            _showDetails = EditorGUILayout.Foldout(_showDetails, "데이터 통계", true, EditorStyles.foldoutHeader);
            
            if (_showDetails)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.LabelField($"Characters: {_gameDatabase.Characters?.Count ?? 0}");
                EditorGUILayout.LabelField($"Stats: {_gameDatabase.Stats?.Count ?? 0}");
                EditorGUILayout.LabelField($"Events: {_gameDatabase.Events?.Count ?? 0}");
                EditorGUILayout.LabelField($"Quests: {_gameDatabase.Quests?.Count ?? 0}");
                EditorGUILayout.LabelField($"Items: {_gameDatabase.Items?.Count ?? 0}");
                EditorGUILayout.LabelField($"Equipment: {_gameDatabase.Equipment?.Count ?? 0}");
                EditorGUILayout.LabelField($"Locations: {_gameDatabase.Locations?.Count ?? 0}");
                EditorGUILayout.LabelField($"Actions: {_gameDatabase.Actions?.Count ?? 0}");
                EditorGUILayout.LabelField($"Endings: {_gameDatabase.Endings?.Count ?? 0}");
                EditorGUILayout.LabelField($"NPC Favors: {_gameDatabase.NPCFavors?.Count ?? 0}");
                EditorGUILayout.LabelField($"NPC Dialogues: {_gameDatabase.NPCDialogues?.Count ?? 0}");
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(10);
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("데이터 관리", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Auto Load All", GUILayout.Height(35)))
            {
                AutoLoadAllData();
            }
            GUI.backgroundColor = Color.white;
            
            if (GUILayout.Button("Clear All", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("확인", "모든 데이터를 삭제하시겠습니까?", "예", "아니오"))
                {
                    ClearAllData();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Load Characters", GUILayout.Height(25)))
            {
                LoadDataFromFolder<CharacterData>("Assets/GameData/Characters/", 
                    data => _gameDatabase.Characters = data);
            }
            
            if (GUILayout.Button("Load Events", GUILayout.Height(25)))
            {
                LoadDataFromFolder<EventData>("Assets/GameData/Events/", 
                    data => _gameDatabase.Events = data);
            }
            
            if (GUILayout.Button("Load Quests", GUILayout.Height(25)))
            {
                LoadDataFromFolder<QuestData>("Assets/GameData/Quests/", 
                    data => _gameDatabase.Quests = data);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Load Items", GUILayout.Height(25)))
            {
                LoadDataFromFolder<ItemData>("Assets/GameData/Items/", 
                    data => _gameDatabase.Items = data);
            }
            
            if (GUILayout.Button("Load Actions", GUILayout.Height(25)))
            {
                LoadDataFromFolder<ActionData>("Assets/GameData/Actions/", 
                    data => _gameDatabase.Actions = data);
            }
            
            if (GUILayout.Button("Load Endings", GUILayout.Height(25)))
            {
                LoadDataFromFolder<EndingData>("Assets/GameData/Endings/", 
                    data => _gameDatabase.Endings = data);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Initialize Cache", GUILayout.Height(30)))
            {
                _gameDatabase.InitializeCache();
                EditorUtility.SetDirty(_gameDatabase);
                Debug.Log("GameDatabase 캐시 초기화 완료");
            }
        }

        private void CreateNewDatabase()
        {
            var database = CreateInstance<GameDatabase>();
            var path = "Assets/GameData/GameDatabase.asset";
            
            // 폴더가 없으면 생성
            if (!AssetDatabase.IsValidFolder("Assets/GameData"))
            {
                AssetDatabase.CreateFolder("Assets", "GameData");
            }
            
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
            
            _gameDatabase = database;
            Selection.activeObject = database;
            Debug.Log($"새로운 GameDatabase 생성: {path}");
        }

        private void FindExistingDatabase()
        {
            var guids = AssetDatabase.FindAssets("t:GameDatabase");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _gameDatabase = AssetDatabase.LoadAssetAtPath<GameDatabase>(path);
                Debug.Log($"GameDatabase 로드: {path}");
            }
            else
            {
                Debug.LogWarning("GameDatabase를 찾을 수 없습니다. 새로 생성하세요.");
            }
        }

        private void AutoLoadAllData()
        {
            if (_gameDatabase == null)
            {
                Debug.LogError("GameDatabase가 선택되지 않았습니다.");
                return;
            }

            LoadDataFromFolder<CharacterData>("Assets/GameData/Characters/", 
                data => _gameDatabase.Characters = data);
            
            LoadDataFromFolder<StatData>("Assets/GameData/Stats/", 
                data => _gameDatabase.Stats = data);
            
            LoadDataFromFolder<EventData>("Assets/GameData/Events/", 
                data => _gameDatabase.Events = data);
            
            LoadDataFromFolder<QuestData>("Assets/GameData/Quests/", 
                data => _gameDatabase.Quests = data);
            
            LoadDataFromFolder<ItemData>("Assets/GameData/Items/", 
                data => _gameDatabase.Items = data);
            
            LoadDataFromFolder<EquipmentData>("Assets/GameData/Equipment/", 
                data => _gameDatabase.Equipment = data);
            
            LoadDataFromFolder<LocationData>("Assets/GameData/Locations/", 
                data => _gameDatabase.Locations = data);
            
            LoadDataFromFolder<ActionData>("Assets/GameData/Actions/", 
                data => _gameDatabase.Actions = data);
            
            LoadDataFromFolder<EndingData>("Assets/GameData/Endings/", 
                data => _gameDatabase.Endings = data);
            
            LoadDataFromFolder<NPCFavorData>("Assets/GameData/NPCs/", 
                data => _gameDatabase.NPCFavors = data);
            
            LoadDataFromFolder<NPCDialogueData>("Assets/GameData/NPCs/Dialogues/", 
                data => _gameDatabase.NPCDialogues = data);

            // 캐시 초기화
            _gameDatabase.InitializeCache();
            
            EditorUtility.SetDirty(_gameDatabase);
            AssetDatabase.SaveAssets();
            
            Debug.Log("모든 데이터 자동 로드 완료!");
        }

        private void LoadDataFromFolder<T>(string folderPath, System.Action<List<T>> setter) where T : ScriptableObject
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"폴더가 존재하지 않습니다: {folderPath}");
                setter(new List<T>());
                return;
            }

            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folderPath });
            var dataList = new List<T>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    dataList.Add(asset);
                }
            }

            setter(dataList);
            Debug.Log($"[{typeof(T).Name}] {dataList.Count}개 로드 완료");
        }

        private void ClearAllData()
        {
            if (_gameDatabase == null) return;

            _gameDatabase.Characters = new List<CharacterData>();
            _gameDatabase.Stats = new List<StatData>();
            _gameDatabase.Events = new List<EventData>();
            _gameDatabase.Quests = new List<QuestData>();
            _gameDatabase.Items = new List<ItemData>();
            _gameDatabase.Equipment = new List<EquipmentData>();
            _gameDatabase.Locations = new List<LocationData>();
            _gameDatabase.Actions = new List<ActionData>();
            _gameDatabase.Endings = new List<EndingData>();
            _gameDatabase.NPCFavors = new List<NPCFavorData>();
            _gameDatabase.NPCDialogues = new List<NPCDialogueData>();

            EditorUtility.SetDirty(_gameDatabase);
            AssetDatabase.SaveAssets();
        }
    }
}
