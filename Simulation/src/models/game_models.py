"""
게임 로직 모델 - 턴, 스탯, 경제, 활동 시스템
"""
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Set, Any
import random
from datetime import datetime


@dataclass
class GameTurn:
    """턴/달력 시스템 (6세 ~ 18세, 144턴)"""
    current_age: int = 6
    current_month: int = 4  # 4월부터 시작 (입학)
    current_year: int = 1
    current_turn: int = 0
    max_turns: int = 144  # 12년 × 12개월
    
    def advance_turn(self):
        """턴 진행"""
        self.current_turn += 1
        self.current_month += 1
        
        if self.current_month > 12:
            self.current_month = 1
            self.current_age += 1
            self.current_year += 1
            
    def is_last_turn(self) -> bool:
        """마지막 턴 체크"""
        return self.current_turn >= self.max_turns or self.current_age >= 18
    
    def get_date_string(self) -> str:
        """현재 날짜 문자열"""
        return f"{self.current_age}세 {self.current_month}월"
    
    def get_progress_percent(self) -> float:
        """게임 진행률 (%)"""
        return (self.current_turn / self.max_turns) * 100


@dataclass
class CharacterStats:
    """캐릭터 스탯 시스템"""
    hp: int = 80
    charm: int = 70
    intelligence: int = 70
    art: int = 70
    morality: int = 80
    stress: int = 0
    
    # 성격 성향 (-100 ~ +100)
    personality: Dict[str, int] = field(default_factory=lambda: {
        'active_calm': 0,
        'independent_dependent': 0,
        'extrovert_introvert': 0,
        'creative_logical': 0
    })
    
    def __post_init__(self):
        """초기화 후 값 검증"""
        self._clamp_stats()
    
    def _clamp_stats(self):
        """스탯 값 범위 제한"""
        self.hp = max(0, min(999, self.hp))
        self.charm = max(0, min(999, self.charm))
        self.intelligence = max(0, min(999, self.intelligence))
        self.art = max(0, min(999, self.art))
        self.morality = max(0, min(999, self.morality))
        self.stress = max(0, min(150, self.stress))
    
    def apply_effects(self, effects: Dict[str, int]):
        """스탯 효과 적용"""
        for stat, value in effects.items():
            stat_upper = stat.upper()
            if stat_upper == 'HP':
                self.hp += value
            elif stat_upper == 'CHARM':
                self.charm += value
            elif stat_upper == 'INT' or stat_upper == 'INTELLIGENCE':
                self.intelligence += value
            elif stat_upper == 'ART':
                self.art += value
            elif stat_upper == 'MORALITY':
                self.morality += value
            elif stat_upper == 'STRESS':
                self.stress += value
                
        self._clamp_stats()
    
    def get_primary_stats(self) -> Dict[str, int]:
        """주요 스탯 반환"""
        return {
            'HP': self.hp,
            'CHARM': self.charm,
            'INT': self.intelligence,
            'ART': self.art,
            'MORALITY': self.morality
        }
    
    def get_total_stats(self) -> int:
        """전체 스탯 합산"""
        return self.hp + self.charm + self.intelligence + self.art + self.morality


@dataclass
class Economy:
    """경제 시스템 (스위트 - 게임 내 화폐)"""
    current_money: int = 1000
    monthly_income: int = 100
    monthly_expense: int = 50
    
    def process_monthly(self):
        """월간 경제 처리"""
        self.current_money += self.monthly_income - self.monthly_expense
        
    def can_afford(self, cost: int) -> bool:
        """구매 가능 여부"""
        return self.current_money >= cost
    
    def spend(self, cost: int) -> bool:
        """지출"""
        if self.can_afford(cost):
            self.current_money -= cost
            return True
        return False
    
    def earn(self, amount: int):
        """수입"""
        self.current_money += amount


@dataclass
class NPCRelationship:
    """NPC 관계 (호감도)"""
    npc_id: str
    current_favor: int = 0
    min_favor: int = 0
    max_favor: int = 100
    is_romanceable: bool = False
    events_triggered: Set[str] = field(default_factory=set)
    
    def __post_init__(self):
        self.current_favor = max(self.min_favor, min(self.max_favor, self.current_favor))
    
    def change_favor(self, amount: int):
        """호감도 변경"""
        self.current_favor = max(self.min_favor, min(self.max_favor, self.current_favor + amount))
    
    def check_threshold(self, threshold: int) -> bool:
        """호감도 임계값 체크"""
        return self.current_favor >= threshold


@dataclass
class MonthlySchedule:
    """월간 일정"""
    activity1: Optional['Activity'] = None
    activity2: Optional['Activity'] = None
    
    def set_schedule(self, act1: 'Activity', act2: 'Activity'):
        """일정 설정"""
        self.activity1 = act1
        self.activity2 = act2
    
    def is_set(self) -> bool:
        """일정이 설정되었는지 확인"""
        return self.activity1 is not None and self.activity2 is not None
    
    def get_total_cost(self) -> int:
        """총 비용"""
        cost = 0
        if self.activity1:
            cost += self.activity1.cost
        if self.activity2:
            cost += self.activity2.cost
        return cost
    
    def get_stat_effects(self) -> Dict[str, int]:
        """통합 스탯 효과"""
        effects = {}
        
        for activity in [self.activity1, self.activity2]:
            if activity:
                for stat, value in activity.stat_effects.items():
                    effects[stat] = effects.get(stat, 0) + value
                    
        return effects


@dataclass
class Activity:
    """활동"""
    activity_id: str
    name_ko: str
    activity_type: str
    cost: int
    min_age: int
    max_age: int
    stat_effects: Dict[str, int] = field(default_factory=dict)
    stress_change: int = 0
    
    def is_available(self, age: int, stats: CharacterStats) -> bool:
        """활동 가능 여부 체크"""
        return self.min_age <= age <= self.max_age


@dataclass
class GameEvent:
    """게임 이벤트"""
    event_id: str
    name_ko: str
    event_type: str
    trigger_conditions: Dict[str, Any] = field(default_factory=dict)
    priority: int = 5
    is_triggered: bool = False
    choices: List['EventChoice'] = field(default_factory=list)
    
    def check_trigger(self, state: 'GameState') -> bool:
        """트리거 조건 체크"""
        if self.is_triggered and not self.trigger_conditions.get('repeatable', False):
            return False
            
        turn = state.turn
        
        # 연령 체크
        age_range = self.trigger_conditions.get('age_range')
        if age_range:
            if not (age_range[0] <= turn.current_age <= age_range[1]):
                return False
        
        # 월/일 체크
        target_month = self.trigger_conditions.get('month')
        if target_month and turn.current_month != target_month:
            return False
            
        target_day = self.trigger_conditions.get('day')
        if target_day and turn.current_turn % 12 * 30 + target_day != turn.current_turn:
            return False
        
        # 스탯 조건 체크
        stat_conditions = self.trigger_conditions.get('stats', {})
        for stat, (min_val, max_val) in stat_conditions.items():
            current = getattr(state.stats, stat.lower(), 0)
            if not (min_val <= current <= max_val):
                return False
        
        # NPC 조건 체크
        npc_conditions = self.trigger_conditions.get('npc_favor', {})
        for npc_id, min_favor in npc_conditions.items():
            if npc_id in state.npc_relations:
                if state.npc_relations[npc_id].current_favor < min_favor:
                    return False
        
        return True
    
    def mark_triggered(self):
        """이벤트 발생 표시"""
        self.is_triggered = True


@dataclass
class EventChoice:
    """이벤트 선택지"""
    choice_id: str
    description: str
    stat_effects: Dict[str, int] = field(default_factory=dict)
    favor_changes: Dict[str, int] = field(default_factory=dict)
    conditions: Dict[str, Any] = field(default_factory=dict)
    
    def is_available(self, state: Optional['GameState']) -> bool:
        """선택 가능 여부"""
        return True


@dataclass
class TurnResult:
    """턴 결과"""
    turn: int
    age: int
    month: int
    activities: List[Activity] = field(default_factory=list)
    triggered_events: List[GameEvent] = field(default_factory=list)
    stat_changes: Dict[str, int] = field(default_factory=dict)
    npc_favor_changes: Dict[str, int] = field(default_factory=dict)


@dataclass
class GameState:
    """전체 게임 상태"""
    turn: GameTurn = field(default_factory=GameTurn)
    stats: CharacterStats = field(default_factory=CharacterStats)
    economy: Economy = field(default_factory=Economy)
    npc_relations: Dict[str, NPCRelationship] = field(default_factory=dict)
    current_schedule: MonthlySchedule = field(default_factory=MonthlySchedule)
    completed_events: Set[str] = field(default_factory=set)
    active_quests: List['Quest'] = field(default_factory=list)
    completed_quests: List[str] = field(default_factory=list)
    turn_history: List[TurnResult] = field(default_factory=list)
    
    def __post_init__(self):
        """초기화"""
        if not self.npc_relations:
            # 기본 NPC 관계 초기화
            self.npc_relations = {
                'Char_Ino': NPCRelationship('Char_Ino', 50, 0, 100, True),
                'Char_Aileen': NPCRelationship('Char_Aileen', 0, 0, 100, True),
                'Char_Kyle': NPCRelationship('Char_Kyle', 30, 0, 100, False),
                'Char_Lian': NPCRelationship('Char_Lian', 0, 50, 100, True),
            }


@dataclass
class Quest:
    """퀘스트"""
    quest_id: str
    name_ko: str
    current_step: int = 0
    total_steps: int = 1
    required_turns: int = 1
    elapsed_turns: int = 0
    
    def is_complete(self) -> bool:
        """퀘스트 완료 여부"""
        return self.current_step >= self.total_steps
    
    def advance_step(self) -> bool:
        """퀘스트 진행"""
        self.current_step += 1
        self.elapsed_turns += 1
        return self.is_complete()


@dataclass
class EndingCondition:
    """엔딩 조건"""
    ending_id: str
    name_ko: str
    priority: int = 1
    stat_conditions: Dict[str, int] = field(default_factory=dict)
    npc_conditions: Dict[str, int] = field(default_factory=dict)
    special_conditions: Dict[str, Any] = field(default_factory=dict)
    
    def check_condition(self, state: GameState) -> bool:
        """엔딩 조건 충족 체크"""
        # 스탯 조건 체크
        for stat, min_value in self.stat_conditions.items():
            current = getattr(state.stats, stat.lower(), 0)
            if current < min_value:
                return False
        
        # NPC 조건 체크
        for npc_id, min_favor in self.npc_conditions.items():
            if npc_id not in state.npc_relations:
                return False
            if state.npc_relations[npc_id].current_favor < min_favor:
                return False
        
        return True
