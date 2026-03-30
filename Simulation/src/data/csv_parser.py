"""
CSV 데이터 파서 - 게임 데이터 로딩
"""
import csv
import os
from typing import Dict, List, Any, Optional
from dataclasses import dataclass, field
from enum import Enum


class ActivityType(Enum):
    STUDY = "study"
    WORK = "work"
    REST = "rest"
    OUTING = "outing"


class StatType(Enum):
    HP = "HP"
    CHARM = "CHARM"
    INT = "INT"
    ART = "ART"
    MORALITY = "MORALITY"
    STRESS = "STRESS"


@dataclass
class Character:
    character_id: str
    name_ko: str
    name_en: str
    essence: str
    age_range: str
    gender: str
    role: str
    personality_traits: List[str]
    description: str
    default_location: str
    favor_min: int
    favor_max: int
    cg_variations: List[str]
    is_romanceable: bool
    unlock_condition: str


@dataclass
class CharacterStat:
    stat_id: str
    name_ko: str
    name_en: str
    description: str
    min_value: int
    max_value: int
    initial_value: int
    growth_rate: float
    is_primary: bool


@dataclass
class Activity:
    activity_id: str
    name_ko: str
    activity_type: ActivityType
    cost: int
    min_age: int
    max_age: int
    stat_effects: Dict[str, int] = field(default_factory=dict)
    stress_change: int = 0
    
    def is_available(self, age: int, stats=None) -> bool:
        """활동 가능 여부 체크"""
        return self.min_age <= age <= self.max_age


@dataclass
class GameEvent:
    event_id: str
    name_ko: str
    event_type: str
    trigger_type: str
    trigger_value: str
    required_previous_event: Optional[str]
    age_min: int
    age_max: int
    month: Optional[int]
    day: Optional[int]
    stat_condition: Optional[str]
    npc_condition: Optional[str]
    priority: int
    description: str


@dataclass
class EndingCondition:
    ending_id: str
    name_ko: str
    priority: int
    stat_conditions: Dict[str, Any] = field(default_factory=dict)
    npc_conditions: Dict[str, Any] = field(default_factory=dict)
    description: str = ""


@dataclass
class Quest:
    quest_id: str
    name_ko: str
    steps: int
    total_turns: int
    trigger_type: str
    trigger_value: str
    required_npc: Optional[str]
    required_favor: int
    required_age: int
    required_stat: Optional[str]
    stat_value: Any
    reward_type: str
    reward_value: str
    description: str


class GameDataLoader:
    """게임 데이터 CSV 로더"""
    
    def __init__(self, data_path: str):
        self.data_path = data_path
        self.characters: Dict[str, Character] = {}
        self.stats: Dict[str, CharacterStat] = {}
        self.events: Dict[str, GameEvent] = {}
        self.endings: Dict[str, EndingCondition] = {}
        self.quests: Dict[str, Quest] = {}
        self.activities: List[Activity] = []
        
    def load_all(self):
        """모든 데이터 로드"""
        self._load_characters()
        self._load_stats()
        self._load_events()
        self._load_endings()
        self._load_quests()
        self._generate_activities_from_stats()
        
    def _parse_csv(self, filename: str) -> List[Dict[str, str]]:
        """CSV 파일 파싱"""
        filepath = os.path.join(self.data_path, filename)
        rows = []
        
        with open(filepath, 'r', encoding='utf-8') as f:
            reader = csv.DictReader(f)
            for row in reader:
                # 주석 라인 스킵
                if any(v and v.startswith('#') for v in row.values()):
                    continue
                # 빈 값 필터링
                clean_row = {k: (v.strip() if v else '') for k, v in row.items()}
                if any(clean_row.values()):
                    rows.append(clean_row)
                    
        return rows
    
    def _parse_int(self, value: str, default: int = 0) -> int:
        """정수 파싱"""
        try:
            return int(value) if value else default
        except:
            return default
    
    def _parse_float(self, value: str, default: float = 0.0) -> float:
        """실수 파싱"""
        try:
            return float(value) if value else default
        except:
            return default
    
    def _parse_bool(self, value: str) -> bool:
        """불리언 파싱"""
        return value.upper() in ['TRUE', '1', 'YES']
    
    def _parse_list(self, value: str, delimiter: str = ';') -> List[str]:
        """리스트 파싱"""
        if not value:
            return []
        return [item.strip() for item in value.split(delimiter) if item.strip()]
    
    def _load_characters(self):
        """캐릭터 데이터 로드"""
        rows = self._parse_csv('Characters.csv')
        
        for row in rows:
            char = Character(
                character_id=row.get('Character_ID', ''),
                name_ko=row.get('Name_KO', ''),
                name_en=row.get('Name_EN', ''),
                essence=row.get('Essence', ''),
                age_range=row.get('Age', ''),
                gender=row.get('Gender', ''),
                role=row.get('Role', ''),
                personality_traits=self._parse_list(row.get('Personality_Traits', '')),
                description=row.get('Description', ''),
                default_location=row.get('Default_Location', ''),
                favor_min=self._parse_int(row.get('Favor_Min', '0')),
                favor_max=self._parse_int(row.get('Favor_Max', '100')),
                cg_variations=self._parse_list(row.get('CG_Variations', '')),
                is_romanceable=self._parse_bool(row.get('Is_Romanceable', 'FALSE')),
                unlock_condition=row.get('Unlock_Condition', 'NONE')
            )
            self.characters[char.character_id] = char
            
    def _load_stats(self):
        """스탯 데이터 로드"""
        rows = self._parse_csv('Character_Stats.csv')
        
        for row in rows:
            stat = CharacterStat(
                stat_id=row.get('Stat_ID', ''),
                name_ko=row.get('Name_KO', ''),
                name_en=row.get('Name_EN', ''),
                description=row.get('Description', ''),
                min_value=self._parse_int(row.get('Min_Value', '0')),
                max_value=self._parse_int(row.get('Max_Value', '999')),
                initial_value=self._parse_int(row.get('Initial_Value', '50')),
                growth_rate=self._parse_float(row.get('Growth_Rate', '1.0')),
                is_primary=self._parse_bool(row.get('Is_Primary', 'TRUE'))
            )
            self.stats[stat.stat_id] = stat
            
    def _load_events(self):
        """이벤트 데이터 로드"""
        rows = self._parse_csv('Events.csv')
        
        for row in rows:
            event = GameEvent(
                event_id=row.get('Event_ID', ''),
                name_ko=row.get('Name_KO', ''),
                event_type=row.get('Type', ''),
                trigger_type=row.get('Trigger_Type', ''),
                trigger_value=row.get('Trigger_Value', ''),
                required_previous_event=row.get('Required_Previous_Event') or None,
                age_min=self._parse_int(row.get('Age_Min', '6')),
                age_max=self._parse_int(row.get('Age_Max', '18')),
                month=self._parse_int(row.get('Month', '')) if row.get('Month') else None,
                day=self._parse_int(row.get('Day', '')) if row.get('Day') else None,
                stat_condition=row.get('Stat_Condition') or None,
                npc_condition=row.get('NPC_Condition') or None,
                priority=self._parse_int(row.get('Priority', '5')),
                description=row.get('Description', '')
            )
            self.events[event.event_id] = event
            
    def _load_endings(self):
        """엔딩 조건 데이터 로드"""
        rows = self._parse_csv('Ending_Conditions.csv')
        
        for row in rows:
            ending = EndingCondition(
                ending_id=row.get('Ending_ID', ''),
                name_ko=row.get('Name_KO', ''),
                priority=self._parse_int(row.get('Priority', '1')),
                description=row.get('Description', '')
            )
            
            # 스탯 조건 파싱
            for i in range(1, 5):
                stat_key = row.get(f'Stat_{i}_Type', '')
                stat_value = row.get(f'Stat_{i}_Value', '')
                if stat_key and stat_value:
                    try:
                        ending.stat_conditions[stat_key] = int(stat_value)
                    except:
                        ending.stat_conditions[stat_key] = stat_value
                        
            self.endings[ending.ending_id] = ending
            
    def _load_quests(self):
        """퀘스트 데이터 로드"""
        rows = self._parse_csv('Quests.csv')
        
        for row in rows:
            quest = Quest(
                quest_id=row.get('Quest_ID', ''),
                name_ko=row.get('Name_KO', ''),
                steps=self._parse_int(row.get('Steps', '1')),
                total_turns=self._parse_int(row.get('Total_Turns', '1')),
                trigger_type=row.get('Trigger_Type', ''),
                trigger_value=row.get('Trigger_Value', ''),
                required_npc=row.get('Required_NPC') or None,
                required_favor=self._parse_int(row.get('Required_Favor', '0')),
                required_age=self._parse_int(row.get('Required_Age', '0')),
                required_stat=row.get('Required_Stat') or None,
                stat_value=row.get('Stat_Value', '0'),
                reward_type=row.get('Reward_Type', ''),
                reward_value=row.get('Reward_Value', ''),
                description=row.get('Description', '')
            )
            self.quests[quest.quest_id] = quest
            
    def _generate_activities_from_stats(self):
        """스탯 데이터 기반으로 활동 생성"""
        stat_activities = {
            'HP': [
                ('ACT_EXERCISE', '운동하기', ActivityType.STUDY, 0, {'HP': 8, 'STRESS': -3}),
                ('ACT_SPORTS', '스포츠 활동', ActivityType.STUDY, 50, {'HP': 12, 'CHARM': 3}),
            ],
            'CHARM': [
                ('ACT_BEAUTY', '미용 관리', ActivityType.STUDY, 100, {'CHARM': 10, 'HP': 2}),
                ('ACT_FASHION', '패션 스터디', ActivityType.STUDY, 80, {'CHARM': 8, 'ART': 3}),
            ],
            'INT': [
                ('ACT_STUDY', '공부하기', ActivityType.STUDY, 0, {'INT': 10, 'STRESS': 5}),
                ('ACT_READING', '독서하기', ActivityType.STUDY, 30, {'INT': 8, 'MORALITY': 2}),
            ],
            'ART': [
                ('ACT_ART', '미술 활동', ActivityType.STUDY, 60, {'ART': 10, 'CHARM': 3}),
                ('ACT_MUSIC', '음악 활동', ActivityType.STUDY, 70, {'ART': 8, 'CHARM': 4}),
            ],
            'MORALITY': [
                ('ACT_VOLUNTEER', '봉사활동', ActivityType.STUDY, 0, {'MORALITY': 10, 'STRESS': -2}),
                ('ACT_MEDITATION', '명상하기', ActivityType.REST, 0, {'MORALITY': 5, 'STRESS': -10}),
            ],
        }
        
        for stat_id, activities in stat_activities.items():
            for act_id, name, act_type, cost, effects in activities:
                activity = Activity(
                    activity_id=act_id,
                    name_ko=name,
                    activity_type=act_type,
                    cost=cost,
                    min_age=6,
                    max_age=18,
                    stat_effects=effects,
                    stress_change=effects.get('STRESS', 0)
                )
                self.activities.append(activity)
                
        # 기본 활동 추가
        self.activities.extend([
            Activity('ACT_REST', '휴식하기', ActivityType.REST, 0, 6, 18, {'STRESS': -15}, -15),
            Activity('ACT_WORK', '아르바이트', ActivityType.WORK, 0, 13, 18, {'HP': -2, 'STRESS': 8}, 8),
            Activity('ACT_OUTING', '외출하기', ActivityType.OUTING, 50, 6, 18, {'CHARM': 3, 'STRESS': -5}, -5),
        ])


# 싱글톤 인스턴스
_data_loader: Optional[GameDataLoader] = None


def get_data_loader(data_path: Optional[str] = None) -> GameDataLoader:
    """데이터 로더 싱글톤 getter"""
    global _data_loader
    if _data_loader is None:
        if data_path is None:
            # Simulation 폴더의 부모(GameProject)로 이동 후 01_GameDesign/Data 찾기
            simulation_dir = os.path.dirname(os.path.dirname(os.path.dirname(__file__)))
            gameproject_dir = os.path.dirname(simulation_dir)
            data_path = os.path.join(gameproject_dir, '01_GameDesign', 'Data')
        _data_loader = GameDataLoader(data_path)
        _data_loader.load_all()
    return _data_loader


def reload_data(data_path: Optional[str] = None) -> GameDataLoader:
    """데이터 리로드"""
    global _data_loader
    _data_loader = None
    return get_data_loader(data_path)
