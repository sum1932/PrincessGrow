using System.Collections.Generic;
using GameData.ScriptableObjects;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 모든 게임 데이터를 관리하는 메인 Database ScriptableObject
    /// 게임 시작 시 Resources에서 로드하거나 직접 참조하여 사용
    /// </summary>
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "GameData/Database", order = 0)]
    public class GameDatabase : ScriptableObject
    {
        [Header("Characters")]
        public List<CharacterData> Characters = new();
        
        [Header("Stats")]
        public List<StatData> Stats = new();
        
        [Header("Events")]
        public List<EventData> Events = new();
        
        [Header("Quests")]
        public List<QuestData> Quests = new();
        
        [Header("Items")]
        public List<ItemData> Items = new();
        
        [Header("Equipment")]
        public List<EquipmentData> Equipment = new();
        
        [Header("Locations")]
        public List<LocationData> Locations = new();
        
        [Header("Actions")]
        public List<ActionData> Actions = new();
        
        [Header("Endings")]
        public List<EndingData> Endings = new();
        
        [Header("NPC")]
        public List<NPCFavorData> NPCFavors = new();
        public List<NPCDialogueData> NPCDialogues = new();

        // 런타임 접근을 위한 Dictionary 캐시
        private Dictionary<string, CharacterData> _characterCache;
        private Dictionary<string, StatData> _statCache;
        private Dictionary<string, EventData> _eventCache;
        private Dictionary<string, QuestData> _questCache;
        private Dictionary<string, ItemData> _itemCache;
        private Dictionary<string, EquipmentData> _equipmentCache;
        private Dictionary<string, LocationData> _locationCache;
        private Dictionary<string, ActionData> _actionCache;
        private Dictionary<string, EndingData> _endingCache;

        /// <summary>
        /// 런타임에 빠른 조회를 위해 캐시 초기화
        /// </summary>
        public void InitializeCache()
        {
            _characterCache = new Dictionary<string, CharacterData>();
            foreach (var c in Characters)
                if (c != null && !_characterCache.ContainsKey(c.CharacterId))
                    _characterCache[c.CharacterId] = c;

            _statCache = new Dictionary<string, StatData>();
            foreach (var s in Stats)
                if (s != null && !_statCache.ContainsKey(s.StatId))
                    _statCache[s.StatId] = s;

            _eventCache = new Dictionary<string, EventData>();
            foreach (var e in Events)
                if (e != null && !_eventCache.ContainsKey(e.EventId))
                    _eventCache[e.EventId] = e;

            _questCache = new Dictionary<string, QuestData>();
            foreach (var q in Quests)
                if (q != null && !_questCache.ContainsKey(q.QuestId))
                    _questCache[q.QuestId] = q;

            _itemCache = new Dictionary<string, ItemData>();
            foreach (var i in Items)
                if (i != null && !_itemCache.ContainsKey(i.ItemId))
                    _itemCache[i.ItemId] = i;

            _equipmentCache = new Dictionary<string, EquipmentData>();
            foreach (var e in Equipment)
                if (e != null && !_equipmentCache.ContainsKey(e.EquipmentId))
                    _equipmentCache[e.EquipmentId] = e;

            _locationCache = new Dictionary<string, LocationData>();
            foreach (var l in Locations)
                if (l != null && !_locationCache.ContainsKey(l.LocationId))
                    _locationCache[l.LocationId] = l;

            _actionCache = new Dictionary<string, ActionData>();
            foreach (var a in Actions)
                if (a != null && !_actionCache.ContainsKey(a.ActionId))
                    _actionCache[a.ActionId] = a;

            _endingCache = new Dictionary<string, EndingData>();
            foreach (var e in Endings)
                if (e != null && !_endingCache.ContainsKey(e.EndingId))
                    _endingCache[e.EndingId] = e;
        }

        #region Data Access Methods

        public CharacterData GetCharacter(string characterId)
        {
            if (_characterCache != null && _characterCache.TryGetValue(characterId, out var character))
                return character;
            return Characters.Find(c => c.CharacterId == characterId);
        }

        public StatData GetStat(string statId)
        {
            if (_statCache != null && _statCache.TryGetValue(statId, out var stat))
                return stat;
            return Stats.Find(s => s.StatId == statId);
        }

        public EventData GetEvent(string eventId)
        {
            if (_eventCache != null && _eventCache.TryGetValue(eventId, out var evt))
                return evt;
            return Events.Find(e => e.EventId == eventId);
        }

        public QuestData GetQuest(string questId)
        {
            if (_questCache != null && _questCache.TryGetValue(questId, out var quest))
                return quest;
            return Quests.Find(q => q.QuestId == questId);
        }

        public ItemData GetItem(string itemId)
        {
            if (_itemCache != null && _itemCache.TryGetValue(itemId, out var item))
                return item;
            return Items.Find(i => i.ItemId == itemId);
        }

        public EquipmentData GetEquipment(string equipmentId)
        {
            if (_equipmentCache != null && _equipmentCache.TryGetValue(equipmentId, out var equip))
                return equip;
            return Equipment.Find(e => e.EquipmentId == equipmentId);
        }

        public LocationData GetLocation(string locationId)
        {
            if (_locationCache != null && _locationCache.TryGetValue(locationId, out var loc))
                return loc;
            return Locations.Find(l => l.LocationId == locationId);
        }

        public ActionData GetAction(string actionId)
        {
            if (_actionCache != null && _actionCache.TryGetValue(actionId, out var action))
                return action;
            return Actions.Find(a => a.ActionId == actionId);
        }

        public EndingData GetEnding(string endingId)
        {
            if (_endingCache != null && _endingCache.TryGetValue(endingId, out var ending))
                return ending;
            return Endings.Find(e => e.EndingId == endingId);
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 특정 나이 범위의 이벤트 검색
        /// </summary>
        public List<EventData> GetEventsByAge(int age)
        {
            return Events.FindAll(e => age >= e.AgeMin && age <= e.AgeMax);
        }

        /// <summary>
        /// 특정 NPC 관련 퀘스트 검색
        /// </summary>
        public List<QuestData> GetQuestsByNPC(string npcId)
        {
            return Quests.FindAll(q => q.RequiredNPC == npcId);
        }

        /// <summary>
        /// 특정 타입의 아이템 검색
        /// </summary>
        public List<ItemData> GetItemsByType(string itemType)
        {
            return Items.FindAll(i => i.Type == itemType);
        }

        /// <summary>
        /// 특정 카테고리의 액션 검색
        /// </summary>
        public List<ActionData> GetActionsByCategory(string category)
        {
            return Actions.FindAll(a => a.Category == category);
        }

        /// <summary>
        /// 특정 타입의 엔딩 검색
        /// </summary>
        public List<EndingData> GetEndingsByType(string endingType)
        {
            return Endings.FindAll(e => e.Type == endingType);
        }

        /// <summary>
        /// NPC의 호감도 레벨 가져오기
        /// </summary>
        public NPCFavorData GetNPCFavor(string npcId)
        {
            return NPCFavors.Find(f => f.NPCId == npcId);
        }

        /// <summary>
        /// 특정 NPC의 대사 검색
        /// </summary>
        public List<NPCDialogueData> GetNPCDialogues(string npcId)
        {
            return NPCDialogues.FindAll(d => d.NPCId == npcId);
        }

        #endregion
    }
}
