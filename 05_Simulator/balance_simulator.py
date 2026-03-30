#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
게임 밸런스 시뮬레이터
스탯 성장, 경제, 이벤트 확률, NPC 호감도, 엔딩 난이도 등의 밸런스를 테스트
- 단일 마크다운 로그 파일 생성
"""

import pandas as pd
import random
from datetime import datetime, timedelta
from pathlib import Path
from typing import Dict, List, Tuple
import statistics

class BalanceSimulator:
    def __init__(self):
        """밸런스 시뮬레이터 초기화"""
        self.simulation_logs = []
        self.results = {}
        self.start_time = datetime.now()
        
    def log(self, message: str, level: str = "INFO"):
        """로그 기록"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.simulation_logs.append({
            'timestamp': timestamp,
            'level': level,
            'message': message
        })
    
    # ==================== 1. 스탯 성장 밸런스 ====================
    def simulate_stat_growth(self, turns: int = 144) -> Dict:
        """스탯 성장 시뮬레이션 (144턴 = 12년)"""
        self.log("=== 스탯 성장 밸런스 시뮬레이션 시작 ===", "START")
        
        stats = {
            'HP': 100, 'Charm': 50, 'Intelligence': 50,
            'Art': 50, 'Morality': 50, 'Stress': 0
        }
        
        stat_history = []
        age = 6
        
        for turn in range(turns):
            # 연령 계산
            if turn > 0 and turn % 12 == 0:
                age += 1
                self.log(f"생일! 루아가 {age}세가 되었습니다!", "EVENT")
            
            # 활동 선택 (랜덤)
            activity = random.choice([
                'study', 'charm_training', 'exercise', 'art', 'morality', 'rest', 'part_time'
            ])
            
            # 활동별 스탯 변화
            changes = self._apply_activity(activity, stats)
            
            stat_history.append({
                'turn': turn + 1,
                'age': age,
                'activity': activity,
                'stats': stats.copy(),
                'changes': changes
            })
            
            if turn % 12 == 0:
                self.log(f"턴 {turn+1}: 연령 {age}세, 활동 {activity}, 스탯 {stats}")
        
        self.log("=== 스탯 성장 밸런스 시뮬레이션 완료 ===", "END")
        
        return {
            'final_stats': stats,
            'stat_history': stat_history,
            'total_turns': turns
        }
    
    def _apply_activity(self, activity: str, stats: Dict) -> Dict:
        """활동 적용 및 스탯 변화 반환"""
        changes = {}
        
        if activity == 'study':
            changes = {'Intelligence': random.randint(5, 12), 'Stress': random.randint(3, 8)}
        elif activity == 'charm_training':
            changes = {'Charm': random.randint(5, 12), 'Stress': random.randint(2, 6)}
        elif activity == 'exercise':
            changes = {'HP': random.randint(8, 15), 'Stress': random.randint(5, 10)}
        elif activity == 'art':
            changes = {'Art': random.randint(5, 12), 'Stress': random.randint(2, 6)}
        elif activity == 'morality':
            changes = {'Morality': random.randint(3, 8), 'Stress': random.randint(-5, -2)}
        elif activity == 'rest':
            changes = {'Stress': random.randint(-15, -8), 'HP': random.randint(3, 8)}
        elif activity == 'part_time':
            changes = {'Stress': random.randint(5, 12)}
        
        for stat, change in changes.items():
            if stat in stats:
                stats[stat] = max(0, min(999, stats[stat] + change))
        
        return changes
    
    # ==================== 2. 경제 밸런스 ====================
    def simulate_economy(self, turns: int = 144, difficulty: str = 'normal') -> Dict:
        """경제 밸런스 시뮬레이션"""
        self.log(f"=== 경제 밸런스 시뮬레이션 시작 (난이도: {difficulty}) ===", "START")
        
        # 난이도별 설정
        difficulty_settings = {
            'easy': {'base_income': 300, 'recommended_expense': 200},
            'normal': {'base_income': 200, 'recommended_expense': 200},
            'hard': {'base_income': 150, 'recommended_expense': 250}
        }
        
        settings = difficulty_settings.get(difficulty, difficulty_settings['normal'])
        money = 1000  # 초기 자금
        money_history = []
        
        part_time_jobs = {
            'convenience': {'income': 150, 'stress': 5, 'efficiency': 30},
            'cafe': {'income': 180, 'stress': 8, 'efficiency': 22.5},
            'gym': {'income': 200, 'stress': 12, 'efficiency': 16.7},
            'bookstore': {'income': 160, 'stress': 3, 'efficiency': 53.3},
            'park': {'income': 100, 'stress': -5, 'efficiency': -20},
            'bakery': {'income': 190, 'stress': 7, 'efficiency': 27.1},
            'tutoring': {'income': 220, 'stress': 10, 'efficiency': 22},
            'model': {'income': 250, 'stress': 15, 'efficiency': 16.7}
        }
        
        for turn in range(turns):
            # 기본 수입
            income = settings['base_income']
            
            # 아르바이트 수입 (30% 확률)
            job_income = 0
            if random.random() < 0.3:
                job = random.choice(list(part_time_jobs.keys()))
                job_income = part_time_jobs[job]['income']
                job_stress = part_time_jobs[job]['stress']
            
            # 지출 (수업비, 생활비)
            expense = random.randint(150, 300)
            
            # 월간 변화
            monthly_change = income + job_income - expense
            money += monthly_change
            money = max(0, money)  # 음수 방지
            
            money_history.append({
                'turn': turn + 1,
                'money': money,
                'income': income + job_income,
                'expense': expense,
                'monthly_change': monthly_change
            })
            
            if turn % 12 == 0:
                self.log(f"턴 {turn+1}: 자금 {money}, 수입 {income + job_income}, 지출 {expense}")
        
        self.log("=== 경제 밸런스 시뮬레이션 완료 ===", "END")
        
        return {
            'final_money': money,
            'money_history': money_history,
            'difficulty': difficulty,
            'average_monthly': statistics.mean([m['monthly_change'] for m in money_history])
        }
    
    # ==================== 3. 이벤트 확률 밸런스 ====================
    def simulate_event_probability(self, trials: int = 1000) -> Dict:
        """이벤트 확률 시뮬레이션"""
        self.log(f"=== 이벤트 확률 밸런스 시뮬레이션 시작 ({trials}회 테스트) ===", "START")
        
        event_rates = {
            'basic': {'probability': 0.20, 'name': '기본 랜덤 이벤트'},
            'high_stress': {'probability': 0.25, 'stress_threshold': 50, 'name': '스트레스 높음 (50+)'},
            'very_high_stress': {'probability': 0.30, 'stress_threshold': 70, 'name': '스트레스 매우 높음 (70+)'},
            'high_stat': {'probability': 0.35, 'stat_threshold': 400, 'name': '특정 스탯 높음 (400+)'},
            'high_favor': {'probability': 0.30, 'favor_threshold': 60, 'name': 'NPC 호감도 높음 (60+)'}
        }
        
        results = {}
        
        for event_type, config in event_rates.items():
            success_count = 0
            
            for _ in range(trials):
                roll = random.random()
                if roll < config['probability']:
                    success_count += 1
            
            actual_rate = success_count / trials
            expected_rate = config['probability']
            
            results[event_type] = {
                'name': config['name'],
                'expected': expected_rate,
                'actual': actual_rate,
                'difference': actual_rate - expected_rate,
                'success_count': success_count
            }
            
            self.log(f"{config['name']}: 예상 {expected_rate:.1%}, 실제 {actual_rate:.1%}, 차이 {actual_rate - expected_rate:+.2%}")
        
        self.log("=== 이벤트 확률 밸런스 시뮬레이션 완료 ===", "END")
        
        return {
            'trials': trials,
            'results': results
        }
    
    # ==================== 4. NPC 호감도 밸런스 ====================
    def simulate_npc_favor(self, turns: int = 144) -> Dict:
        """NPC 호감도 밸런스 시뮬레이션"""
        self.log("=== NPC 호감도 밸런스 시뮬레이션 시작 ===", "START")
        
        npc_favor = {'Ino': 20, 'Aileen': 20, 'Kyle': 20, 'Lian': 0}
        favor_history = []
        
        # 호감도 레벨 경계
        level_thresholds = {
            1: (0, 20),
            2: (20, 40),
            3: (40, 60),
            4: (60, 80),
            5: (80, 100)
        }
        
        for turn in range(turns):
            # NPC 상호작용 (50% 확률)
            if random.random() < 0.5:
                npc = random.choice(list(npc_favor.keys()))
                interaction_type = random.choice(['dialogue', 'dialogue', 'gift', 'event'])
                
                # 호감도 변화
                if interaction_type == 'dialogue':
                    favor_change = random.randint(3, 8)
                elif interaction_type == 'gift':
                    favor_change = random.randint(8, 20)
                else:  # event
                    favor_change = random.randint(5, 15)
                
                old_favor = npc_favor[npc]
                npc_favor[npc] = min(100, npc_favor[npc] + favor_change)
                
                # 레벨 계산
                old_level = self._get_favor_level(old_favor)
                new_level = self._get_favor_level(npc_favor[npc])
                
                favor_history.append({
                    'turn': turn + 1,
                    'npc': npc,
                    'type': interaction_type,
                    'change': favor_change,
                    'before': old_favor,
                    'after': npc_favor[npc],
                    'level_up': new_level > old_level
                })
                
                if new_level > old_level:
                    self.log(f"턴 {turn+1}: {npc} 레벨 업! {old_level} → {new_level}", "EVENT")
            
            if turn % 24 == 0:
                self.log(f"턴 {turn+1}: NPC 호감도 {npc_favor}")
        
        self.log("=== NPC 호감도 밸런스 시뮬레이션 완료 ===", "END")
        
        return {
            'final_favor': npc_favor,
            'favor_history': favor_history,
            'total_interactions': len(favor_history)
        }
    
    def _get_favor_level(self, favor: int) -> int:
        """호감도 레벨 계산"""
        if favor >= 80: return 5
        elif favor >= 60: return 4
        elif favor >= 40: return 3
        elif favor >= 20: return 2
        else: return 1
    
    # ==================== 5. 엔딩 달성 난이도 ====================
    def simulate_ending_difficulty(self, simulations: int = 100) -> Dict:
        """엔딩 달성 난이도 시뮬레이션"""
        self.log(f"=== 엔딩 달성 난이도 시뮬레이션 시작 ({simulations}회) ===", "START")
        
        endings = {
            'easy': [
                {'name': '평범한 행복', 'clear_rate': 1.0, 'expected_turns': (80, 100)},
                {'name': '균형잡힌 인재', 'clear_rate': 0.8, 'expected_turns': (80, 100)},
                {'name': '사교적 리더', 'clear_rate': 0.7, 'expected_turns': (100, 120)}
            ],
            'normal': [
                {'name': '현명한 학자', 'clear_rate': 0.6, 'expected_turns': (100, 120)},
                {'name': '예술가의 길', 'clear_rate': 0.6, 'expected_turns': (100, 120)},
                {'name': '운동선수', 'clear_rate': 0.6, 'expected_turns': (100, 120)},
                {'name': '따뜻한 교사', 'clear_rate': 0.55, 'expected_turns': (110, 130)},
                {'name': '요리 연구가', 'clear_rate': 0.55, 'expected_turns': (110, 130)},
                {'name': '인간 세계 탐구자', 'clear_rate': 0.5, 'expected_turns': (120, 140)},
                {'name': '성스러운 성녀', 'clear_rate': 0.5, 'expected_turns': (120, 140)}
            ],
            'hard': [
                {'name': '여왕의 길', 'clear_rate': 0.4, 'expected_turns': (130, 144)},
                {'name': '반짝이는 아이돌', 'clear_rate': 0.4, 'expected_turns': (130, 144)},
                {'name': '자유로운 영혼', 'clear_rate': 0.35, 'expected_turns': (130, 144)}
            ],
            'hidden': [
                {'name': '차원의 연결자', 'clear_rate': 0.15, 'expected_turns': (140, 144)},
                {'name': '새로운 여왕', 'clear_rate': 0.10, 'expected_turns': (140, 144)}
            ]
        }
        
        results = {}
        
        for difficulty, ending_list in endings.items():
            difficulty_results = []
            
            for ending in ending_list:
                success_count = 0
                turn_counts = []
                
                for _ in range(simulations):
                    # 엔딩 달성 시뮬레이션
                    if random.random() < ending['clear_rate']:
                        success_count += 1
                        turn_count = random.randint(*ending['expected_turns'])
                        turn_counts.append(turn_count)
                
                actual_rate = success_count / simulations
                avg_turns = statistics.mean(turn_counts) if turn_counts else 0
                
                difficulty_results.append({
                    'name': ending['name'],
                    'expected_rate': ending['clear_rate'],
                    'actual_rate': actual_rate,
                    'expected_turns': ending['expected_turns'],
                    'avg_turns': avg_turns,
                    'success_count': success_count
                })
                
                self.log(f"[{difficulty}] {ending['name']}: 예상 {ending['clear_rate']:.0%}, 실제 {actual_rate:.0%}, 평균 {avg_turns:.0f}턴")
            
            results[difficulty] = difficulty_results
        
        self.log("=== 엔딩 달성 난이도 시뮬레이션 완료 ===", "END")
        
        return {
            'simulations': simulations,
            'results': results
        }
    
    # ==================== 마크다운 보고서 생성 ====================
    def generate_markdown_report(self, output_path: str = None):
        """통합 마크다운 보고서 생성"""
        if output_path is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            output_path = f"C:\\우수민\\Maker\\GameProject\\Balance_Simulation_Report_{timestamp}.md"
        
        # 모든 시뮬레이션 실행
        self.log("모든 밸런스 시뮬레이션 시작", "START")
        
        # 1. 스탯 성장
        stat_result = self.simulate_stat_growth(turns=144)
        
        # 2. 경제 밸런스 (3가지 난이도)
        economy_results = {}
        for difficulty in ['easy', 'normal', 'hard']:
            economy_results[difficulty] = self.simulate_economy(turns=144, difficulty=difficulty)
        
        # 3. 이벤트 확률
        event_result = self.simulate_event_probability(trials=1000)
        
        # 4. NPC 호감도
        npc_result = self.simulate_npc_favor(turns=144)
        
        # 5. 엔딩 난이도
        ending_result = self.simulate_ending_difficulty(simulations=100)
        
        self.log("모든 밸런스 시뮬레이션 완료", "END")
        
        # 마크다운 생성
        md_content = []
        
        # 헤더
        md_content.append("# 게임 밸런스 시뮬레이션 통합 보고서")
        md_content.append(f"\n**생성 시간:** {datetime.now().strftime('%Y년 %m월 %d일 %H:%M:%S')}")
        md_content.append(f"**총 시뮬레이션 시간:** {(datetime.now() - self.start_time).total_seconds():.2f}초")
        md_content.append("\n---\n")
        
        # 목차
        md_content.append("## 목차")
        md_content.append("1. [스탯 성장 밸런스](#1-스탯-성장-밸런스)")
        md_content.append("2. [경제 밸런스](#2-경제-밸런스)")
        md_content.append("3. [이벤트 확률 밸런스](#3-이벤트-확률-밸런스)")
        md_content.append("4. [NPC 호감도 밸런스](#4-npc-호감도-밸런스)")
        md_content.append("5. [엔딩 달성 난이도](#5-엔딩-달성-난이도)")
        md_content.append("6. [종합 분석 및 권장사항](#6-종합-분석-및-권장사항)")
        md_content.append("7. [상세 로그](#7-상세-로그)")
        md_content.append("\n---\n")
        
        # 1. 스탯 성장 밸런스
        md_content.append("## 1. 스탯 성장 밸런스\n")
        md_content.append("### 1.1 최종 스탯")
        md_content.append("| 스탯 | 초기 값 | 최종 값 | 총 상승 | 평균 월간 상승 |")
        md_content.append("|------|---------|---------|---------|----------------|")
        initial_stats = {'HP': 100, 'Charm': 50, 'Intelligence': 50, 'Art': 50, 'Morality': 50, 'Stress': 0}
        for stat, final_value in stat_result['final_stats'].items():
            initial = initial_stats.get(stat, 0)
            total_gain = final_value - initial
            monthly_avg = total_gain / stat_result['total_turns'] if stat_result['total_turns'] > 0 else 0
            md_content.append(f"| {stat} | {initial} | {final_value} | +{total_gain} | +{monthly_avg:.1f} |")
        md_content.append("")
        
        md_content.append("### 1.2 연령별 스탯 추이")
        md_content.append("| 연령 | HP | Charm | Intelligence | Art | Morality | Stress |")
        md_content.append("|------|-----|-------|--------------|-----|----------|--------|")
        age_stats = {}
        for record in stat_result['stat_history']:
            age = record['age']
            if age not in age_stats:
                age_stats[age] = record['stats']
        for age in sorted(age_stats.keys()):
            stats = age_stats[age]
            md_content.append(f"| {age}세 | {stats['HP']} | {stats['Charm']} | {stats['Intelligence']} | {stats['Art']} | {stats['Morality']} | {stats['Stress']} |")
        md_content.append("")
        
        # 2. 경제 밸런스
        md_content.append("## 2. 경제 밸런스\n")
        md_content.append("### 2.1 난이도별 자금 흐름")
        md_content.append("| 난이도 | 최종 자금 | 평균 월간 변화 | 추천 월간 수입 | 추천 월간 지출 |")
        md_content.append("|--------|-----------|----------------|----------------|----------------|")
        difficulty_names = {'easy': '쉬움', 'normal': '보통', 'hard': '어려움'}
        for difficulty, result in economy_results.items():
            md_content.append(f"| {difficulty_names.get(difficulty, difficulty)} | {result['final_money']} | {result['average_monthly']:+.0f} | {result['difficulty'] == 'easy' and 300 or (result['difficulty'] == 'normal' and 200 or 150)} | {result['difficulty'] == 'easy' and 200 or (result['difficulty'] == 'normal' and 200 or 250)} |")
        md_content.append("")
        
        md_content.append("### 2.2 아르바이트 수익성")
        md_content.append("| 아르바이트 | 수입 | 스트레스 | 수익/스트레스 비율 |")
        md_content.append("|------------|------|----------|-------------------|")
        jobs = {
            '편의점': (150, 5), '카페': (180, 8), '체육관': (200, 12),
            '서점': (160, 3), '공원 청소': (100, -5), '베이커리': (190, 7),
            '튜터링': (220, 10), '모델': (250, 15)
        }
        for job, (income, stress) in jobs.items():
            ratio = income / stress if stress != 0 else float('inf')
            md_content.append(f"| {job} | {income} | {stress} | {ratio:.1f} |")
        md_content.append("")
        
        # 3. 이벤트 확률 밸런스
        md_content.append("## 3. 이벤트 확률 밸런스\n")
        md_content.append(f"**테스트 횟수:** {event_result['trials']}회\n")
        md_content.append("| 이벤트 타입 | 예상 확률 | 실제 확률 | 차이 | 성공 횟수 |")
        md_content.append("|-------------|-----------|-----------|------|-----------|")
        for event_type, result in event_result['results'].items():
            difference = result['difference']
            diff_str = f"{difference:+.2%}"
            md_content.append(f"| {result['name']} | {result['expected']:.0%} | {result['actual']:.1%} | {diff_str} | {result['success_count']} |")
        md_content.append("")
        
        # 4. NPC 호감도 밸런스
        md_content.append("## 4. NPC 호감도 밸런스\n")
        md_content.append("### 4.1 최종 NPC 호감도")
        md_content.append("| NPC | 초기 호감도 | 최종 호감도 | 상승량 | 최종 레벨 | 상호작용 횟수 |")
        md_content.append("|-----|-------------|-------------|--------|-----------|---------------|")
        initial_favor = {'Ino': 20, 'Aileen': 20, 'Kyle': 20, 'Lian': 0}
        for npc, final_favor in npc_result['final_favor'].items():
            initial = initial_favor.get(npc, 0)
            gain = final_favor - initial
            level = self._get_favor_level(final_favor)
            interactions = len([h for h in npc_result['favor_history'] if h['npc'] == npc])
            md_content.append(f"| {npc} | {initial} | {final_favor} | +{gain} | Level {level} | {interactions} |")
        md_content.append("")
        
        md_content.append("### 4.2 레벨업 이력")
        level_ups = [h for h in npc_result['favor_history'] if h['level_up']]
        if level_ups:
            md_content.append("| 턴 | NPC | 이전 레벨 | 새 레벨 | 호감도 |")
            md_content.append("|-----|-----|-----------|---------|--------|")
            for record in level_ups[:10]:  # 최근 10개만
                md_content.append(f"| {record['turn']} | {record['npc']} | {self._get_favor_level(record['before'])} | {self._get_favor_level(record['after'])} | {record['after']} |")
        md_content.append("")
        
        md_content.append("### 4.3 호감도 상승 효율")
        md_content.append("| 활동 | 호감도 상승 범위 | 평균 상승 |")
        md_content.append("|------|------------------|-----------|")
        md_content.append("| 대화 | 3~8 | 5.5 |")
        md_content.append("| 선물 | 8~20 | 14.0 |")
        md_content.append("| 이벤트 | 5~15 | 10.0 |")
        md_content.append("")
        
        md_content.append("### 4.4 레벨 업 필요량")
        md_content.append("| 레벨 | 필요 호감도 | 누적 | 예상 소요 턴 |")
        md_content.append("|------|-------------|------|-------------|")
        md_content.append("| 1→2 | 20 | 20 | 2~4턴 |")
        md_content.append("| 2→3 | 20 | 40 | 4~8턴 |")
        md_content.append("| 3→4 | 20 | 60 | 6~12턴 |")
        md_content.append("| 4→5 | 20 | 80 | 8~16턴 |")
        md_content.append("| 5→Max | 20 | 100 | 10~20턴 |")
        md_content.append("")
        
        # 5. 엔딩 달성 난이도
        md_content.append("## 5. 엔딩 달성 난이도\n")
        md_content.append(f"**시뮬레이션 횟수:** {ending_result['simulations']}회/엔딩\n")
        
        difficulty_kr = {'easy': '쉬움', 'normal': '보통', 'hard': '어려움', 'hidden': '히든'}
        for difficulty, endings in ending_result['results'].items():
            md_content.append(f"### 5.{list(ending_result['results'].keys()).index(difficulty) + 1} {difficulty_kr.get(difficulty, difficulty)} ({len(endings)}개)")
            md_content.append("| 엔딩명 | 예상 성공률 | 실제 성공률 | 예상 턴 | 평균 클리어 턴 |")
            md_content.append("|--------|-------------|-------------|---------|----------------|")
            for ending in endings:
                md_content.append(f"| {ending['name']} | {ending['expected_rate']:.0%} | {ending['actual_rate']:.0%} | {ending['expected_turns'][0]}~{ending['expected_turns'][1]} | {ending['avg_turns']:.0f} |")
            md_content.append("")
        
        # 6. 종합 분석
        md_content.append("## 6. 종합 분석 및 권장사항\n")
        
        md_content.append("### 6.1 밸런스 평가")
        md_content.append("#### 스탯 성장")
        total_stat_gain = sum([stat_result['final_stats'][s] - initial_stats[s] for s in initial_stats.keys() if s != 'Stress'])
        avg_monthly = total_stat_gain / 144 / 5  # 5개 스탯 평균
        md_content.append(f"- 총 스탯 상승량: {total_stat_gain} (월간 평균: +{avg_monthly:.1f})")
        md_content.append(f"- 목표: 월간 평균 +8~12 (6~9세), +12~18 (10~14세), +15~25 (15~17세)")
        if avg_monthly < 8:
            md_content.append("- ⚠️ **권장:** 스탯 상승률이 목표보다 낮습니다. 활동 효율을 상향 조정하세요.")
        elif avg_monthly > 25:
            md_content.append("- ⚠️ **권장:** 스탯 상승률이 목표보다 높습니다. 활동 효율을 하향 조정하세요.")
        else:
            md_content.append("- ✅ 스탯 상승률이 적정 범위입니다.")
        md_content.append("")
        
        md_content.append("#### 경제 밸런스")
        normal_final = economy_results['normal']['final_money']
        md_content.append(f"- 보통 난이도 최종 자금: {normal_final}")
        if normal_final < 0:
            md_content.append("- ⚠️ **권장:** 보통 난이도에서 자금이 부족합니다. 수입을 늘리거나 지출을 줄이세요.")
        elif normal_final > 5000:
            md_content.append("- ⚠️ **권장:** 보통 난이도에서 자금이 과잉입니다. 지출을 늘리거나 수입을 줄이세요.")
        else:
            md_content.append("- ✅ 경제 밸런스가 적정합니다.")
        md_content.append("")
        
        md_content.append("#### 이벤트 확률")
        md_content.append("- 랜덤 이벤트 확률이 예상값과 일치하는지 확인하세요.")
        md_content.append("- 스트레스/스탯 조건에 따른 확률 조정이 적절히 작동하는지 확인하세요.")
        md_content.append("")
        
        md_content.append("#### NPC 호감도")
        avg_favor = sum(npc_result['final_favor'].values()) / len(npc_result['final_favor'])
        md_content.append(f"- 평균 NPC 호감도: {avg_favor:.1f}")
        if avg_favor < 40:
            md_content.append("- ⚠️ **권장:** 평균 호감도가 낮습니다. 상호작용 빈도나 상승량을 늘리세요.")
        elif avg_favor > 80:
            md_content.append("- ⚠️ **권장:** 평균 호감도가 높습니다. 상승량을 줄이거나 필요 호감도를 높이세요.")
        else:
            md_content.append("- ✅ NPC 호감도 밸런스가 적정합니다.")
        md_content.append("")
        
        md_content.append("#### 엔딩 난이도")
        easy_endings = [e for e in ending_result['results']['easy'] if e['actual_rate'] > 0.9]
        hard_endings = [e for e in ending_result['results']['hard'] if e['actual_rate'] < 0.3]
        md_content.append(f"- 쉬운 엔딩 중 과도하게 쉬운: {len(easy_endings)}개")
        md_content.append(f"- 어려운 엔딩 중 과도하게 어려운: {len(hard_endings)}개")
        if easy_endings:
            md_content.append(f"- ⚠️ **권장:** {', '.join([e['name'] for e in easy_endings[:3]])} 등의 성공률을 낮추세요.")
        if hard_endings:
            md_content.append(f"- ⚠️ **권장:** {', '.join([e['name'] for e in hard_endings[:3]])} 등의 성공률을 높이세요.")
        md_content.append("")
        
        md_content.append("### 6.2 조정 권장사항")
        md_content.append("1. **스탯 성장:** 수업/활동별 효율성을 Data와 Document_Tasks.md의 BalanceSheet 기준과 비교")
        md_content.append("2. **경제:** 아르바이트 수익성과 수업 비용 밸런스 재검토")
        md_content.append("3. **이벤트:** 랜덤 이벤트 발생 확률이 플레이어 경험에 적절한지 확인")
        md_content.append("4. **NPC:** 호감도 상승 난이도와 레벨업 필요량 조정")
        md_content.append("5. **엔딩:** 히든 엔딩 달성 조건이 너무 어렵지 않은지 확인")
        md_content.append("")
        
        # 7. 상세 로그
        md_content.append("## 7. 상세 로그\n")
        md_content.append("<details>")
        md_content.append("<summary>전체 시뮬레이션 로그 보기 (클릭)</summary>")
        md_content.append("")
        md_content.append("```")
        for log in self.simulation_logs:
            md_content.append(f"[{log['level']}] {log['message']}")
        md_content.append("```")
        md_content.append("</details>")
        
        # 파일 저장
        output_path = Path(output_path)
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(md_content))
        
        print(f"\n{'='*70}")
        print(f"밸런스 시뮬레이션 완료!")
        print(f"보고서 저장 위치: {output_path}")
        print(f"{'='*70}")
        
        return output_path


def run_balance_simulation():
    """밸런스 시뮬레이션 실행"""
    print("="*70)
    print("게임 밸런스 시뮬레이션 시작")
    print("="*70)
    
    sim = BalanceSimulator()
    report_path = sim.generate_markdown_report()
    
    print(f"\n보고서가 생성되었습니다: {report_path}")
    
    return sim, report_path


if __name__ == '__main__':
    sim, report_path = run_balance_simulation()
