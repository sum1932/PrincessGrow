"""
시뮬레이션 엔진 - 100회 실행, 10회마다 결과 기록
"""
import random
import copy
from typing import List, Dict, Any, Optional
from dataclasses import dataclass, field
from datetime import datetime

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from models.game_models import (
    GameState, GameTurn, CharacterStats, Economy, Activity,
    GameEvent, EventChoice, TurnResult, NPCRelationship, 
    Quest, EndingCondition
)
from data.csv_parser import get_data_loader, ActivityType


@dataclass
class SingleSimulationResult:
    """단일 시뮬레이션 결과"""
    simulation_number: int
    ending_id: Optional[str]
    ending_name: str
    total_turns: int
    final_age: int
    final_stats: Dict[str, int]
    final_stress: int
    npc_favors: Dict[str, int]
    triggered_events: List[str]
    completed_quests: List[str]
    final_money: int
    is_success: bool


@dataclass
class SimulationBatchResult:
    """배치 시뮬레이션 결과 (10회 단위)"""
    batch_number: int
    start_sim: int
    end_sim: int
    results: List[SingleSimulationResult]
    statistics: Dict[str, Any] = field(default_factory=dict)


@dataclass
class FullSimulationReport:
    """전체 시뮬레이션 보고서"""
    total_simulations: int
    batch_size: int
    execution_time: float
    timestamp: str
    data_source: str
    batches: List[SimulationBatchResult]
    overall_statistics: Dict[str, Any] = field(default_factory=dict)
    ending_distribution: Dict[str, int] = field(default_factory=dict)


class SimulationStrategy:
    """시뮬레이션 전략 (활동 선택 및 이벤트 선택)"""
    
    def __init__(self, activities: List[Activity], strategy_type: str = "random"):
        self.activities = activities
        self.strategy_type = strategy_type
        self.random = random.Random()
        
    def select_activities(self, state: GameState) -> tuple[Optional[Activity], Optional[Activity]]:
        """활동 선택"""
        available = [a for a in self.activities if a.is_available(state.turn.current_age, state.stats)]
        
        if not available:
            return None, None
        
        if self.strategy_type == "random":
            # 랜덤 선택
            act1 = self.random.choice(available)
            act2 = self.random.choice(available)
            return act1, act2
        
        elif self.strategy_type == "balanced":
            # 균형 잡힌 선택 (낮은 스탯 우선)
            stats = state.stats.get_primary_stats()
            lowest_stat = min(stats.items(), key=lambda x: x[1])[0]
            
            # 해당 스탯을 올려주는 활동 우선 선택
            stat_activities = [a for a in available if lowest_stat in a.stat_effects]
            if stat_activities:
                act1 = self.random.choice(stat_activities)
            else:
                act1 = self.random.choice(available)
            
            act2 = self.random.choice(available)
            return act1, act2
        
        elif self.strategy_type == "stress_aware":
            # 스트레스 관리 우선
            if state.stats.stress > 80:
                # 스트레스 낮추는 활동 선택
                rest_activities = [a for a in available if a.stress_change < 0]
                if rest_activities:
                    return self.random.choice(rest_activities), self.random.choice(rest_activities)
            
            return self.random.choice(available), self.random.choice(available)
        
        return self.random.choice(available), self.random.choice(available)
    
    def make_event_choice(self, event: GameEvent, choices: List[EventChoice]) -> Optional[EventChoice]:
        """이벤트 선택지 결정"""
        if not choices:
            return None
        
        available = [c for c in choices if c.is_available(None)]
        if not available:
            return choices[0] if choices else None
        
        return self.random.choice(available)


class GameSimulator:
    """게임 시뮬레이션 엔진"""
    
    def __init__(self, data_loader=None):
        self.data_loader = data_loader or get_data_loader()
        self.strategy: Optional[SimulationStrategy] = None
        self.events: List[GameEvent] = []
        self.endings: List[EndingCondition] = []
        self._initialize_events()
        self._initialize_endings()
        
    def _initialize_events(self):
        """이벤트 초기화"""
        for event_id, event_data in self.data_loader.events.items():
            event = GameEvent(
                event_id=event_data.event_id,
                name_ko=event_data.name_ko,
                event_type=event_data.event_type,
                priority=event_data.priority,
                trigger_conditions={
                    'age_range': (event_data.age_min, event_data.age_max),
                    'month': event_data.month,
                    'day': event_data.day,
                }
            )
            self.events.append(event)
        
        # 우선순위 정렬
        self.events.sort(key=lambda e: e.priority, reverse=True)
    
    def _initialize_endings(self):
        """엔딩 조건 초기화"""
        for ending_id, ending_data in self.data_loader.endings.items():
            ending = EndingCondition(
                ending_id=ending_data.ending_id,
                name_ko=ending_data.name_ko,
                priority=ending_data.priority,
                stat_conditions=ending_data.stat_conditions,
                npc_conditions=ending_data.npc_conditions
            )
            self.endings.append(ending)
        
        # 우선순위 정렬
        self.endings.sort(key=lambda e: e.priority, reverse=True)
    
    def set_strategy(self, strategy_type: str = "random"):
        """전략 설정"""
        self.strategy = SimulationStrategy(self.data_loader.activities, strategy_type)
    
    def run_single_simulation(self, sim_number: int) -> SingleSimulationResult:
        """단일 시뮬레이션 실행"""
        # 게임 상태 초기화
        state = GameState()
        
        # 이벤트 상태 리셋
        for event in self.events:
            event.is_triggered = False
        
        triggered_events = []
        max_turns = 144
        consecutive_failures = 0
        
        while not state.turn.is_last_turn() and state.turn.current_turn < max_turns:
            # 활동 선택
            act1, act2 = self.strategy.select_activities(state)
            
            if act1 is None or act2 is None:
                consecutive_failures += 1
                if consecutive_failures >= 3:
                    break
                # 기본 생활로 진행
                state.turn.advance_turn()
                state.economy.process_monthly()
                continue
            
            # 비용 체크
            total_cost = act1.cost + act2.cost
            if not state.economy.can_afford(total_cost):
                # 자금 부족 시 저렴한 활동 선택
                cheap_activities = [a for a in self.data_loader.activities 
                                   if a.cost == 0 and a.is_available(state.turn.current_age, state.stats)]
                if len(cheap_activities) >= 2:
                    act1, act2 = cheap_activities[0], cheap_activities[1]
                else:
                    state.turn.advance_turn()
                    state.economy.process_monthly()
                    continue
            
            # 비용 지불
            state.economy.spend(act1.cost + act2.cost)
            
            # 스탯 효과 적용
            effects = {}
            for act in [act1, act2]:
                for stat, value in act.stat_effects.items():
                    effects[stat] = effects.get(stat, 0) + value
            
            state.stats.apply_effects(effects)
            
            # 월간 경제 처리
            state.economy.process_monthly()
            
            # 이벤트 체크
            turn_events = []
            for event in self.events:
                if event.check_trigger(state):
                    event.mark_triggered()
                    triggered_events.append(event.name_ko)
                    turn_events.append(event)
                    
                    # 선택지 처리
                    if event.choices:
                        choice = self.strategy.make_event_choice(event, event.choices)
                        if choice:
                            state.stats.apply_effects(choice.stat_effects)
                            for npc_id, favor in choice.favor_changes.items():
                                if npc_id in state.npc_relations:
                                    state.npc_relations[npc_id].change_favor(favor)
            
            # 턴 기록
            turn_result = TurnResult(
                turn=state.turn.current_turn,
                age=state.turn.current_age,
                month=state.turn.current_month,
                activities=[act1, act2],
                triggered_events=turn_events,
                stat_changes=effects,
            )
            state.turn_history.append(turn_result)
            
            # 턴 진행
            state.turn.advance_turn()
            consecutive_failures = 0
            
            # 랜덤 이벤트 (20% 확률)
            if random.random() < 0.2:
                npc_id = random.choice(list(state.npc_relations.keys()))
                favor_change = random.randint(-5, 10)
                state.npc_relations[npc_id].change_favor(favor_change)
        
        # 엔딩 판정
        ending = self._evaluate_ending(state)
        
        return SingleSimulationResult(
            simulation_number=sim_number,
            ending_id=ending.ending_id if ending else None,
            ending_name=ending.name_ko if ending else "미달성",
            total_turns=state.turn.current_turn,
            final_age=state.turn.current_age,
            final_stats=state.stats.get_primary_stats(),
            final_stress=state.stats.stress,
            npc_favors={npc_id: rel.current_favor for npc_id, rel in state.npc_relations.items()},
            triggered_events=triggered_events,
            completed_quests=state.completed_quests,
            final_money=state.economy.current_money,
            is_success=ending is not None
        )
    
    def _evaluate_ending(self, state: GameState) -> Optional[EndingCondition]:
        """엔딩 판정"""
        for ending in self.endings:
            if ending.check_condition(state):
                return ending
        return None
    
    def run_batch_simulations(
        self, 
        total_simulations: int = 100,
        batch_size: int = 10,
        strategy_type: str = "random"
    ) -> FullSimulationReport:
        """배치 시뮬레이션 실행"""
        import time
        
        start_time = time.time()
        self.set_strategy(strategy_type)
        
        batches = []
        all_results = []
        
        for batch_num in range(total_simulations // batch_size):
            batch_results = []
            start_sim = batch_num * batch_size + 1
            end_sim = (batch_num + 1) * batch_size
            
            print(f"\n[배치 {batch_num + 1}] 시뮬레이션 {start_sim} ~ {end_sim} 실행 중...")
            
            for sim_num in range(start_sim, end_sim + 1):
                result = self.run_single_simulation(sim_num)
                batch_results.append(result)
                all_results.append(result)
                
                # 진행 상황 출력
                if sim_num % 5 == 0:
                    print(f"  - 시뮬레이션 {sim_num}/{total_simulations} 완료")
            
            # 배치 통계 계산
            batch_stats = self._calculate_batch_statistics(batch_results)
            
            batch_result = SimulationBatchResult(
                batch_number=batch_num + 1,
                start_sim=start_sim,
                end_sim=end_sim,
                results=batch_results,
                statistics=batch_stats
            )
            batches.append(batch_result)
            
            print(f"[배치 {batch_num + 1}] 완료 - 평균 스탯: {batch_stats.get('avg_total_stats', 0):.1f}")
        
        execution_time = time.time() - start_time
        
        # 전체 통계 계산
        overall_stats = self._calculate_overall_statistics(all_results)
        ending_dist = self._calculate_ending_distribution(all_results)
        
        report = FullSimulationReport(
            total_simulations=total_simulations,
            batch_size=batch_size,
            execution_time=execution_time,
            timestamp=datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            data_source=self.data_loader.data_path,
            batches=batches,
            overall_statistics=overall_stats,
            ending_distribution=ending_dist
        )
        
        return report
    
    def _calculate_batch_statistics(self, results: List[SingleSimulationResult]) -> Dict[str, Any]:
        """배치 통계 계산"""
        if not results:
            return {}
        
        total_stats_list = [sum(r.final_stats.values()) for r in results]
        stress_list = [r.final_stress for r in results]
        money_list = [r.final_money for r in results]
        
        stats_by_type = {stat: [] for stat in ['HP', 'CHARM', 'INT', 'ART', 'MORALITY']}
        for r in results:
            for stat, value in r.final_stats.items():
                if stat in stats_by_type:
                    stats_by_type[stat].append(value)
        
        return {
            'count': len(results),
            'success_count': sum(1 for r in results if r.is_success),
            'success_rate': sum(1 for r in results if r.is_success) / len(results) * 100,
            'avg_total_stats': sum(total_stats_list) / len(total_stats_list),
            'avg_stress': sum(stress_list) / len(stress_list),
            'avg_money': sum(money_list) / len(money_list),
            'stat_averages': {stat: sum(values) / len(values) if values else 0 
                            for stat, values in stats_by_type.items()},
            'ending_types': self._calculate_ending_distribution(results)
        }
    
    def _calculate_overall_statistics(self, results: List[SingleSimulationResult]) -> Dict[str, Any]:
        """전체 통계 계산"""
        if not results:
            return {}
        
        total_stats_list = [sum(r.final_stats.values()) for r in results]
        stress_list = [r.final_stress for r in results]
        money_list = [r.final_money for r in results]
        turns_list = [r.total_turns for r in results]
        
        return {
            'total_simulations': len(results),
            'successful_endings': sum(1 for r in results if r.is_success),
            'success_rate': sum(1 for r in results if r.is_success) / len(results) * 100,
            'avg_total_stats': sum(total_stats_list) / len(total_stats_list),
            'avg_stress': sum(stress_list) / len(stress_list),
            'avg_money': sum(money_list) / len(money_list),
            'avg_turns': sum(turns_list) / len(turns_list),
            'max_total_stats': max(total_stats_list),
            'min_total_stats': min(total_stats_list),
        }
    
    def _calculate_ending_distribution(self, results: List[SingleSimulationResult]) -> Dict[str, int]:
        """엔딩 분포 계산"""
        distribution = {}
        for r in results:
            ending_name = r.ending_name if r.ending_name else "미달성"
            distribution[ending_name] = distribution.get(ending_name, 0) + 1
        return distribution
