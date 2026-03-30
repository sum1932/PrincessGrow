#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
게임 밸런스 시뮬레이터 (Data CSV 연동 버전)
Data 폴더의 CSV 파일을 읽어 실제 게임 데이터로 밸런스 시뮬레이션
- 단일 마크다운 로그 파일 생성
"""

import pandas as pd
import random
from datetime import datetime, timedelta
from pathlib import Path
from typing import Dict, List, Tuple
import statistics

class BalanceSimulator:
    def __init__(self, data_dir: str = None):
        """밸런스 시뮬레이터 초기화"""
        if data_dir is None:
            data_dir = "C:\\우수민\\Maker\\GameProject\\01_GameDesign\\Data"
        
        self.data_dir = Path(data_dir)
        self.simulation_logs = []
        self.results = {}
        self.start_time = datetime.now()
        
        # 데이터 로드
        self.actions_df = self._load_csv("Actions.csv")
        self.stats_df = self._load_csv("Character_Stats.csv")
        self.npc_favor_df = self._load_csv("NPC_Favor.csv")
        self.items_df = self._load_csv("Items_Master.csv")
        
        # 초기화
        self.initial_stats = self._get_initial_stats()
        self.npc_favor_config = self._get_npc_favor_config()
        
    def _load_csv(self, filename: str) -> pd.DataFrame:
        """CSV 파일 로드"""
        try:
            filepath = self.data_dir / filename
            # 불량 라인 건너뛰기
            df = pd.read_csv(filepath, comment='#', encoding='utf-8', on_bad_lines='skip')
            
            # 컬럼명 정리 (공백 제거)
            df.columns = df.columns.str.strip()
            
            self.log(f"데이터 로드 성공: {filename} ({len(df)} rows, columns: {list(df.columns)})")
            return df
        except Exception as e:
            self.log(f"데이터 로드 실패: {filename} - {e}", "WARN")
            import traceback
            traceback.print_exc()
            return pd.DataFrame()
    
    def _get_initial_stats(self) -> Dict:
        """Character_Stats.csv에서 초기 스탯 가져오기"""
        stats = {}
        for _, row in self.stats_df.iterrows():
            stat_id = row['Stat_ID']
            initial = row.get('Initial_Value', 0)
            stats[stat_id] = int(initial)
        self.log(f"초기 스탯 로드: {stats}")
        return stats
    
    def _get_npc_favor_config(self) -> Dict:
        """NPC_Favor.csv에서 호감도 설정 가져오기"""
        config = {}
        for _, row in self.npc_favor_df.iterrows():
            npc_name = row['NPC_Name']
            if npc_name not in config:
                config[npc_name] = {
                    'levels': [],
                    'min_start': 999,
                    'max_start': 0
                }
            
            level = int(row['Favor_Level'])
            min_favor = int(row['Min_Favor'])
            max_favor = int(row['Max_Favor'])
            
            config[npc_name]['levels'].append({
                'level': level,
                'min': min_favor,
                'max': max_favor
            })
            config[npc_name]['min_start'] = min(config[npc_name]['min_start'], min_favor)
            config[npc_name]['max_start'] = max(config[npc_name]['max_start'], max_favor)
        
        self.log(f"NPC 호감도 설정 로드: {list(config.keys())}")
        return config
    
    def _get_activity_effects(self, action_id: str) -> Dict:
        """Actions.csv에서 활동 효과 가져오기"""
        action = self.actions_df[self.actions_df['Action_ID'] == action_id]
        if action.empty:
            return {}
        
        row = action.iloc[0]
        effects = {}
        
        # 스탯 효과 매핑
        stat_mapping = {
            'Effect_HP': 'HP',
            'Effect_Charm': 'CHARM',
            'Effect_Int': 'INT',
            'Effect_Art': 'ART',
            'Effect_Morality': 'MORALITY',
            'Effect_Stress': 'STRESS'
        }
        
        for col, stat_id in stat_mapping.items():
            if col in row and pd.notna(row[col]):
                effects[stat_id] = int(row[col])
        
        # 수입/비용
        if 'Income_Sweets' in row and pd.notna(row[col]):
            effects['income'] = int(row['Income_Sweets'])
        if 'Cost_Sweets' in row and pd.notna(row['Cost_Sweets']):
            effects['cost'] = int(row['Cost_Sweets'])
        
        return effects
    
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
        self.log("=== 스탯 성장 밸런스 시뮬레이션 시작 (Data 연동) ===", "START")
        
        # Character_Stats.csv에서 초기 스탯 사용
        stats = self.initial_stats.copy()
        
        # Actions.csv에서 활동 목록 가져오기
        lessons = self.actions_df[self.actions_df['Category'] == 'Lesson']['Action_ID'].tolist()
        jobs = self.actions_df[self.actions_df['Category'] == 'Job']['Action_ID'].tolist()
        rest = ['ACT_Rest']  # 휴식은 별도 처리
        
        stat_history = []
        age = 6
        
        for turn in range(turns):
            if turn > 0 and turn % 12 == 0:
                age += 1
                self.log(f"생일! 루아가 {age}세가 되었습니다!", "EVENT")
            
            # 활동 선택 (60% 수업, 20% 아르바이트, 20% 휴식)
            rand = random.random()
            if rand < 0.6 and lessons:
                activity_id = random.choice(lessons)
                activity_type = 'Lesson'
            elif rand < 0.8 and jobs:
                activity_id = random.choice(jobs)
                activity_type = 'Job'
            else:
                activity_id = 'ACT_Rest'
                activity_type = 'Rest'
            
            # Data 기반 효과 적용
            if activity_type == 'Rest':
                changes = {'STRESS': random.randint(-15, -8)}
            else:
                effects = self._get_activity_effects(activity_id)
                # 약간의 랜덤 변동성 추가
                changes = {}
                for stat, value in effects.items():
                    if stat in ['HP', 'CHARM', 'INT', 'ART', 'MORALITY', 'STRESS']:
                        variation = random.uniform(0.8, 1.2)
                        changes[stat] = int(value * variation)
            
            # 스탯 적용
            for stat, change in changes.items():
                if stat in stats:
                    max_val = 999 if stat != 'STRESS' else 100
                    stats[stat] = max(0, min(max_val, stats[stat] + change))
            
            stat_history.append({
                'turn': turn + 1,
                'age': age,
                'activity_id': activity_id,
                'activity_type': activity_type,
                'stats': stats.copy(),
                'changes': changes
            })
            
            if turn % 12 == 0:
                self.log(f"턴 {turn+1}: 연령 {age}세, 활동 {activity_id}, 스탯 HP={stats.get('HP',0)} CHARM={stats.get('CHARM',0)} INT={stats.get('INT',0)}")
        
        self.log("=== 스탯 성장 밸런스 시뮬레이션 완료 ===", "END")
        
        return {
            'final_stats': stats,
            'stat_history': stat_history,
            'total_turns': turns
        }
    
    # ==================== 2. 경제 밸런스 ====================
    def simulate_economy(self, turns: int = 144, difficulty: str = 'normal') -> Dict:
        """경제 밸런스 시뮬레이션"""
        self.log(f"=== 경제 밸런스 시뮬레이션 시작 (난이도: {difficulty}) ===", "START")
        
        difficulty_settings = {
            'easy': {'base_income': 300, 'recommended_expense': 200},
            'normal': {'base_income': 200, 'recommended_expense': 200},
            'hard': {'base_income': 150, 'recommended_expense': 250}
        }
        
        settings = difficulty_settings.get(difficulty, difficulty_settings['normal'])
        money = 1000
        money_history = []
        
        # Actions.csv에서 아르바이트 데이터 가져오기
        jobs_df = self.actions_df[self.actions_df['Category'] == 'Job']
        
        for turn in range(turns):
            income = settings['base_income']
            job_income = 0
            job_stress = 0
            
            # 아르바이트 (30% 확률)
            if random.random() < 0.3 and not jobs_df.empty:
                job_row = jobs_df.sample(1).iloc[0]
                job_income = int(job_row.get('Income_Sweets', 0))
                job_stress = int(job_row.get('Effect_Stress', 0))
            
            # 수업 비용
            lesson_cost = 0
            if random.random() < 0.7:  # 70% 확률로 수업
                lessons_df = self.actions_df[self.actions_df['Category'] == 'Lesson']
                if not lessons_df.empty:
                    lesson_row = lessons_df.sample(1).iloc[0]
                    lesson_cost = int(lesson_row.get('Cost_Sweets', 0))
            
            # 생활비
            living_cost = random.randint(50, 100)
            
            total_expense = lesson_cost + living_cost
            monthly_change = income + job_income - total_expense
            money += monthly_change
            money = max(0, money)
            
            money_history.append({
                'turn': turn + 1,
                'money': money,
                'income': income + job_income,
                'expense': total_expense,
                'monthly_change': monthly_change,
                'lesson_cost': lesson_cost,
                'job_income': job_income
            })
            
            if turn % 12 == 0:
                self.log(f"턴 {turn+1}: 자금 {money}, 수입 {income + job_income}, 지출 {total_expense}")
        
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
            
            self.log(f"{config['name']}: 예상 {expected_rate:.1%}, 실제 {actual_rate:.1%}")
        
        self.log("=== 이벤트 확률 밸런스 시뮬레이션 완료 ===", "END")
        
        return {'trials': trials, 'results': results}
    
    # ==================== 4. NPC 호감도 밸런스 ====================
    def simulate_npc_favor(self, turns: int = 144) -> Dict:
        """NPC 호감도 밸런스 시뮬레이션"""
        self.log("=== NPC 호감도 밸런스 시뮬레이션 시작 (Data 연동) ===", "START")
        
        # NPC_Favor.csv에서 초기값 설정
        npc_favor = {}
        for npc_name, config in self.npc_favor_config.items():
            npc_favor[npc_name] = config['min_start']
        
        favor_history = []
        
        for turn in range(turns):
            if random.random() < 0.5:  # 50% 확률로 상호작용
                npc = random.choice(list(npc_favor.keys()))
                interaction_type = random.choice(['dialogue', 'dialogue', 'gift', 'event'])
                
                # Data 기반 상승량 (BalanceSheet 기준)
                if interaction_type == 'dialogue':
                    favor_change = random.randint(3, 8)
                elif interaction_type == 'gift':
                    favor_change = random.randint(8, 20)
                else:
                    favor_change = random.randint(5, 15)
                
                old_favor = npc_favor[npc]
                npc_favor[npc] = min(150, npc_favor[npc] + favor_change)  # Max 150
                
                old_level = self._get_npc_level(npc, old_favor)
                new_level = self._get_npc_level(npc, npc_favor[npc])
                
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
    
    def _get_npc_level(self, npc_name: str, favor: int) -> int:
        """NPC_Favor.csv 기준으로 레벨 계산"""
        if npc_name not in self.npc_favor_config:
            return 1
        
        levels = self.npc_favor_config[npc_name]['levels']
        for level_info in sorted(levels, key=lambda x: x['level'], reverse=True):
            if favor >= level_info['min']:
                return level_info['level']
        return 1
    
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
                
                self.log(f"[{difficulty}] {ending['name']}: 예상 {ending['clear_rate']:.0%}, 실제 {actual_rate:.0%}")
            
            results[difficulty] = difficulty_results
        
        self.log("=== 엔딩 달성 난이도 시뮬레이션 완료 ===", "END")
        
        return {'simulations': simulations, 'results': results}
    
    # ==================== 마크다운 보고서 생성 ====================
    def generate_markdown_report(self, output_path: str = None):
        """통합 마크다운 보고서 생성"""
        if output_path is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            output_path = f"C:\\우수민\\Maker\\GameProject\\Balance_Simulation_Data_Report_{timestamp}.md"
        
        self.log("모든 밸런스 시뮬레이션 시작 (Data 연동)", "START")
        
        # 1. 스탯 성장
        stat_result = self.simulate_stat_growth(turns=144)
        
        # 2. 경제 밸런스
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
        md_content.append("# 게임 밸런스 시뮬레이션 보고서 (Data 연동)")
        md_content.append(f"\n**생성 시간:** {datetime.now().strftime('%Y년 %m월 %d일 %H:%M:%S')}")
        md_content.append(f"**총 시뮬레이션 시간:** {(datetime.now() - self.start_time).total_seconds():.2f}초")
        md_content.append(f"**데이터 출처:** {self.data_dir}")
        md_content.append("\n---\n")
        
        # 목차
        md_content.append("## 목차")
        md_content.append("1. [데이터 연동 정보](#1-데이터-연동-정보)")
        md_content.append("2. [스탯 성장 밸런스](#2-스탯-성장-밸런스)")
        md_content.append("3. [경제 밸런스](#3-경제-밸런스)")
        md_content.append("4. [이벤트 확률 밸런스](#4-이벤트-확률-밸런스)")
        md_content.append("5. [NPC 호감도 밸런스](#5-npc-호감도-밸런스)")
        md_content.append("6. [엔딩 달성 난이도](#6-엔딩-달성-난이도)")
        md_content.append("7. [종합 분석 및 권장사항](#7-종합-분석-및-권장사항)")
        md_content.append("8. [상세 로그](#8-상세-로그)")
        md_content.append("\n---\n")
        
        # 1. 데이터 연동 정보
        md_content.append("## 1. 데이터 연동 정보\n")
        md_content.append("### 1.1 로드된 데이터 파일")
        md_content.append("| 파일명 | 행 수 | 상태 |")
        md_content.append("|--------|-------|------|")
        for filename in ['Actions.csv', 'Character_Stats.csv', 'NPC_Favor.csv', 'Items_Master.csv']:
            df = getattr(self, f'{filename.replace(".csv", "").lower()}_df', None)
            if df is not None:
                status = "✅ 로드됨" if not df.empty else "⚠️ 비어있음"
                md_content.append(f"| {filename} | {len(df)} | {status} |")
        md_content.append("")
        
        md_content.append("### 1.2 초기 스탯 (Character_Stats.csv)")
        md_content.append("| 스탯 ID | 초기 값 | 최대 값 |")
        md_content.append("|---------|---------|---------|")
        for _, row in self.stats_df.iterrows():
            md_content.append(f"| {row['Stat_ID']} | {row.get('Initial_Value', 'N/A')} | {row.get('Max_Value', 'N/A')} |")
        md_content.append("")
        
        md_content.append("### 1.3 NPC 레벨 설정 (NPC_Favor.csv)")
        md_content.append("| NPC | 레벨 | 호감도 범위 |")
        md_content.append("|-----|------|-------------|")
        for npc_name, config in self.npc_favor_config.items():
            for level_info in config['levels']:
                md_content.append(f"| {npc_name} | {level_info['level']} | {level_info['min']}~{level_info['max']} |")
        md_content.append("")
        
        # 2. 스탯 성장
        md_content.append("## 2. 스탯 성장 밸런스\n")
        md_content.append("### 2.1 최종 스탯 (Actions.csv 기반)")
        md_content.append("| 스탯 | 초기 값 | 최종 값 | 총 상승 | 평균 월간 상승 |")
        md_content.append("|------|---------|---------|---------|----------------|")
        for stat_id in ['HP', 'CHARM', 'INT', 'ART', 'MORALITY', 'STRESS']:
            initial = self.initial_stats.get(stat_id, 0)
            final = stat_result['final_stats'].get(stat_id, 0)
            gain = final - initial
            monthly = gain / 144 if 144 > 0 else 0
            md_content.append(f"| {stat_id} | {initial} | {final} | {'+' if gain > 0 else ''}{gain} | {'+' if monthly > 0 else ''}{monthly:.1f} |")
        md_content.append("")
        
        md_content.append("### 2.2 연령별 스탯 추이")
        md_content.append("| 연령 | HP | CHARM | INT | ART | MORALITY | STRESS |")
        md_content.append("|------|-----|-------|-----|-----|----------|--------|")
        age_stats = {}
        for record in stat_result['stat_history']:
            age = record['age']
            if age not in age_stats:
                age_stats[age] = record['stats']
        for age in sorted(age_stats.keys()):
            stats = age_stats[age]
            md_content.append(f"| {age}세 | {stats.get('HP',0)} | {stats.get('CHARM',0)} | {stats.get('INT',0)} | {stats.get('ART',0)} | {stats.get('MORALITY',0)} | {stats.get('STRESS',0)} |")
        md_content.append("")
        
        md_content.append("### 2.3 주요 활동별 스탯 효과 (Actions.csv)")
        md_content.append("| 활동 ID | 이름 | HP | CHARM | INT | ART | MORALITY | STRESS | 수입 | 비용 |")
        md_content.append("|---------|------|-----|-------|-----|-----|----------|--------|------|------|")
        for _, row in self.actions_df.iterrows():
            md_content.append(f"| {row.get('Action_ID','')} | {row.get('Name_KO','')} | "
                            f"{row.get('Effect_HP',0)} | {row.get('Effect_Charm',0)} | "
                            f"{row.get('Effect_Int',0)} | {row.get('Effect_Art',0)} | "
                            f"{row.get('Effect_Morality',0)} | {row.get('Effect_Stress',0)} | "
                            f"{row.get('Income_Sweets',0)} | {row.get('Cost_Sweets',0)} |")
        md_content.append("")
        
        # 3. 경제 밸런스
        md_content.append("## 3. 경제 밸런스\n")
        md_content.append("### 3.1 난이도별 자금 흐름")
        md_content.append("| 난이도 | 최종 자금 | 평균 월간 변화 |")
        md_content.append("|--------|-----------|----------------|")
        difficulty_names = {'easy': '쉬움', 'normal': '보통', 'hard': '어려움'}
        for difficulty, result in economy_results.items():
            md_content.append(f"| {difficulty_names.get(difficulty, difficulty)} | {result['final_money']} | {result['average_monthly']:+.0f} |")
        md_content.append("")
        
        md_content.append("### 3.2 아르바이트 수익성 (Actions.csv)")
        jobs_df = self.actions_df[self.actions_df['Category'] == 'Job']
        md_content.append("| 아르바이트 | 수입 | 스트레스 | 수익/스트레스 |")
        md_content.append("|------------|------|----------|---------------|")
        for _, row in jobs_df.iterrows():
            income = int(row.get('Income_Sweets', 0))
            stress = int(row.get('Effect_Stress', 0))
            ratio = income / stress if stress != 0 else 0
            md_content.append(f"| {row.get('Name_KO','')} | {income} | {stress} | {ratio:.1f} |")
        md_content.append("")
        
        # 4. 이벤트 확률
        md_content.append("## 4. 이벤트 확률 밸런스\n")
        md_content.append(f"**테스트 횟수:** {event_result['trials']}회\n")
        md_content.append("| 이벤트 타입 | 예상 확률 | 실제 확률 | 차이 |")
        md_content.append("|-------------|-----------|-----------|------|")
        for event_type, result in event_result['results'].items():
            difference = result['difference']
            diff_str = f"{difference:+.2%}"
            md_content.append(f"| {result['name']} | {result['expected']:.0%} | {result['actual']:.1%} | {diff_str} |")
        md_content.append("")
        
        # 5. NPC 호감도
        md_content.append("## 5. NPC 호감도 밸런스\n")
        md_content.append("### 5.1 최종 NPC 호감도")
        md_content.append("| NPC | 초기 호감도 | 최종 호감도 | 상승량 | 최종 레벨 | 상호작용 횟수 |")
        md_content.append("|-----|-------------|-------------|--------|-----------|---------------|")
        initial_favor = {npc: config['min_start'] for npc, config in self.npc_favor_config.items()}
        for npc, final_favor in npc_result['final_favor'].items():
            initial = initial_favor.get(npc, 0)
            gain = final_favor - initial
            level = self._get_npc_level(npc, final_favor)
            interactions = len([h for h in npc_result['favor_history'] if h['npc'] == npc])
            md_content.append(f"| {npc} | {initial} | {final_favor} | +{gain} | Level {level} | {interactions} |")
        md_content.append("")
        
        md_content.append("### 5.2 레벨업 이력")
        level_ups = [h for h in npc_result['favor_history'] if h['level_up']]
        if level_ups:
            md_content.append("| 턴 | NPC | 이전 레벨 | 새 레벨 | 호감도 |")
            md_content.append("|-----|-----|-----------|---------|--------|")
            for record in level_ups[:10]:
                md_content.append(f"| {record['turn']} | {record['npc']} | {self._get_npc_level(record['npc'], record['before'])} | {self._get_npc_level(record['npc'], record['after'])} | {record['after']} |")
        md_content.append("")
        
        # 6. 엔딩
        md_content.append("## 6. 엔딩 달성 난이도\n")
        md_content.append(f"**시뮬레이션 횟수:** {ending_result['simulations']}회/엔딩\n")
        difficulty_kr = {'easy': '쉬움', 'normal': '보통', 'hard': '어려움', 'hidden': '히든'}
        for difficulty, endings in ending_result['results'].items():
            md_content.append(f"### 6.{list(ending_result['results'].keys()).index(difficulty) + 1} {difficulty_kr.get(difficulty, difficulty)} ({len(endings)}개)")
            md_content.append("| 엔딩명 | 예상 성공률 | 실제 성공률 | 평균 클리어 턴 |")
            md_content.append("|--------|-------------|-------------|----------------|")
            for ending in endings:
                md_content.append(f"| {ending['name']} | {ending['expected_rate']:.0%} | {ending['actual_rate']:.0%} | {ending['avg_turns']:.0f} |")
            md_content.append("")
        
        # 7. 종합 분석
        md_content.append("## 7. 종합 분석 및 권장사항\n")
        
        # 스탯 분석
        total_gain = 0
        primary_stats = ['HP', 'CHARM', 'INT', 'ART', 'MORALITY']
        for stat_id in primary_stats:
            initial = self.initial_stats.get(stat_id, 0)
            final = stat_result['final_stats'].get(stat_id, 0)
            total_gain += (final - initial)
        avg_monthly = total_gain / 144 / len(primary_stats) if 144 > 0 else 0
        
        md_content.append("### 7.1 밸런스 평가")
        md_content.append("#### 스탯 성장")
        md_content.append(f"- 총 스탯 상승량: {total_gain} (월간 평균: +{avg_monthly:.1f})")
        md_content.append(f"- 목표: 월간 평균 +8~12 (6~9세), +12~18 (10~14세), +15~25 (15~17세)")
        
        if avg_monthly < 8:
            md_content.append("- ⚠️ **권장:** 스탯 상승률이 목표보다 낮습니다.")
            md_content.append("  - Actions.csv의 Effect_* 컬럼 값을 증가시키세요.")
            md_content.append("  - 수업 빈도를 높이거나 효율성을 상향 조정하세요.")
        elif avg_monthly > 25:
            md_content.append("- ⚠️ **권장:** 스탯 상승률이 목표보다 높습니다.")
            md_content.append("  - Actions.csv의 Effect_* 컬럼 값을 감소시키세요.")
        else:
            md_content.append("- ✅ 스탯 상승률이 적정 범위입니다.")
        md_content.append("")
        
        # 경제 분석
        normal_final = economy_results['normal']['final_money']
        md_content.append("#### 경제 밸런스")
        md_content.append(f"- 보통 난이도 최종 자금: {normal_final}")
        if normal_final < 0:
            md_content.append("- ⚠️ **권장:** 자금이 부족합니다.")
            md_content.append("  - Actions.csv의 Income_Sweets를 증가시키거나 Cost_Sweets를 감소시키세요.")
        elif normal_final > 10000:
            md_content.append("- ⚠️ **권장:** 자금이 과잉입니다.")
            md_content.append("  - Actions.csv의 Cost_Sweets를 증가시키세요.")
        else:
            md_content.append("- ✅ 경제 밸런스가 적정합니다.")
        md_content.append("")
        
        # NPC 분석
        avg_favor = sum(npc_result['final_favor'].values()) / len(npc_result['final_favor'])
        md_content.append("#### NPC 호감도")
        md_content.append(f"- 평균 NPC 호감도: {avg_favor:.1f}")
        max_level_npcs = [npc for npc, favor in npc_result['final_favor'].items() if self._get_npc_level(npc, favor) >= 5]
        if len(max_level_npcs) >= 2:
            md_content.append(f"- ⚠️ **권장:** {len(max_level_npcs)}개 NPC가 최대 레벨에 도달했습니다.")
            md_content.append("  - NPC_Favor.csv의 Max_Favor 값을 증가시키세요.")
            md_content.append("  - 호감도 상승량을 줄이세요.")
        md_content.append("")
        
        # 8. 조정 가이드
        md_content.append("### 7.2 CSV 파일 조정 가이드")
        md_content.append("스탯 조정 시 다음 파일들을 수정하세요:")
        md_content.append("")
        md_content.append("**Actions.csv:**")
        md_content.append("- `Effect_HP`, `Effect_Charm`, `Effect_Int`, `Effect_Art`, `Effect_Morality`: 스탯 변화량")
        md_content.append("- `Effect_Stress`: 스트레스 변화량 (음수는 감소)")
        md_content.append("- `Income_Sweets`: 아르바이트 수입")
        md_content.append("- `Cost_Sweets`: 수업 비용")
        md_content.append("")
        md_content.append("**NPC_Favor.csv:**")
        md_content.append("- `Min_Favor`, `Max_Favor`: 각 레벨의 호감도 범위")
        md_content.append("- `Favor_Level`: 레벨 번호 (1~5)")
        md_content.append("")
        md_content.append("**Character_Stats.csv:**")
        md_content.append("- `Initial_Value`: 게임 시작 시 초기 스탯")
        md_content.append("- `Max_Value`: 스탯 최대치")
        md_content.append("")
        
        # 9. 로그
        md_content.append("## 8. 상세 로그\n")
        md_content.append("<details>")
        md_content.append("<summary>전체 시뮬레이션 로그 보기 (클릭)</summary>")
        md_content.append("")
        md_content.append("```")
        for log in self.simulation_logs:
            md_content.append(f"[{log['level']}] {log['message']}")
        md_content.append("```")
        md_content.append("</details>")
        
        # 저장
        output_path = Path(output_path)
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(md_content))
        
        print(f"\n{'='*70}")
        print(f"밸런스 시뮬레이션 완료! (Data 연동)")
        print(f"보고서 저장 위치: {output_path}")
        print(f"{'='*70}")
        
        return output_path


def run_balance_simulation_with_data():
    """Data 연동 밸런스 시뮬레이션 실행"""
    print("="*70)
    print("게임 밸런스 시뮬레이션 시작 (Data 연동)")
    print("="*70)
    
    sim = BalanceSimulator()
    report_path = sim.generate_markdown_report()
    
    print(f"\n보고서가 생성되었습니다: {report_path}")
    
    return sim, report_path


if __name__ == '__main__':
    sim, report_path = run_balance_simulation_with_data()
