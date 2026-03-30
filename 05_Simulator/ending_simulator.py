#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
엔딩 분기/달성 시뮬레이터
Ending_Conditions.csv 데이터를 기반으로 엔딩 달성 조건을 시뮬레이션
- CSV 파일 연동
- 마크다운 로그 파일 생성
"""

import pandas as pd
import random
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import re

class EndingSimulator:
    def __init__(self, data_dir: str = None):
        """엔딩 시뮬레이터 초기화"""
        if data_dir is None:
            data_dir = "C:\\우수민\\Maker\\GameProject\\01_GameDesign\\Data"
        
        self.data_dir = Path(data_dir)
        self.simulation_logs = []
        self.start_time = datetime.now()
        
        # 데이터 로드
        self.endings_df = self._load_csv("Ending_Conditions.csv")
        self.stats_df = self._load_csv("Character_Stats.csv")
        
        # 게임 상태 초기화
        self.reset_game_state()
        
    def _load_csv(self, filename: str) -> pd.DataFrame:
        """CSV 파일 로드"""
        try:
            filepath = self.data_dir / filename
            df = pd.read_csv(filepath, comment='#', encoding='utf-8', on_bad_lines='skip')
            df.columns = df.columns.str.strip()
            self.log(f"데이터 로드 성공: {filename} ({len(df)} rows)")
            return df
        except Exception as e:
            self.log(f"데이터 로드 실패: {filename} - {e}", "WARN")
            return pd.DataFrame()
    
    def reset_game_state(self):
        """게임 상태 초기화"""
        # 스탯 초기화 (Character_Stats.csv에서)
        self.stats = {}
        for _, row in self.stats_df.iterrows():
            stat_id = row['Stat_ID']
            initial = row.get('Initial_Value', 0)
            self.stats[stat_id] = int(initial)
        
        # NPC 호감도 - NPC_Favor.csv 기반 (리안은 50부터 시작, 나머지는 0)
        self.npc_favor = {'Ino': 0, 'Aileen': 0, 'Kyle': 0, 'Lian': 50}
        
        # 완료된 이벤트/플래그
        self.completed_events = set()
        self.flags = set()
        
        # 게임 진행 정보
        self.age = 6
        self.turn = 0
        self.max_turns = 144  # 12년
        
        self.log("게임 상태 초기화 완료")
        self.log(f"초기 NPC 호감도: {self.npc_favor}")
        
        self.log("게임 상태 초기화 완료")
        self.log(f"초기 NPC 호감도: {self.npc_favor}")
    
    def log(self, message: str, level: str = "INFO"):
        """로그 기록"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.simulation_logs.append({
            'timestamp': timestamp,
            'level': level,
            'message': message
        })
    
    def parse_condition(self, condition_str: str) -> Tuple[str, int]:
        """조건 문자열 파싱 (예: '500+' -> ('>=', 500))"""
        if pd.isna(condition_str) or condition_str == '':
            return None, 0
        
        condition_str = str(condition_str).strip()
        
        # 500+ 형식
        if condition_str.endswith('+'):
            value = int(condition_str[:-1])
            return '>=', value
        
        # 100 형식 (정확히 일치)
        if condition_str.isdigit():
            return '=', int(condition_str)
        
        # <=30, >=80 형식
        match = re.match(r'([<>]=?|=)(\d+)', condition_str)
        if match:
            operator = match.group(1)
            value = int(match.group(2))
            return operator, value
        
        return None, 0
    
    def check_stat_condition(self, ending: pd.Series) -> Tuple[bool, Dict]:
        """엔딩 스탯 조건 체크"""
        checks = {}
        all_passed = True
        
        stat_checks = [
            ('Req_HP', 'HP'),
            ('Req_Charm', 'CHARM'),
            ('Req_Int', 'INT'),
            ('Req_Art', 'ART'),
            ('Req_Morality', 'MORALITY'),
            ('Req_Stress', 'STRESS')
        ]
        
        for req_col, stat_key in stat_checks:
            if req_col in ending and pd.notna(ending[req_col]):
                operator, required = self.parse_condition(ending[req_col])
                if operator:
                    current = self.stats.get(stat_key, 0)
                    
                    if operator == '>=':
                        passed = current >= required
                    elif operator == '<=':
                        passed = current <= required
                    elif operator == '>':
                        passed = current > required
                    elif operator == '<':
                        passed = current < required
                    else:  # '='
                        passed = current == required
                    
                    checks[stat_key] = {
                        'required': f"{operator}{required}",
                        'current': current,
                        'passed': passed
                    }
                    
                    if not passed:
                        all_passed = False
        
        return all_passed, checks
    
    def check_npc_condition(self, ending: pd.Series) -> Tuple[bool, Dict]:
        """엔딩 NPC 호감도 조건 체크"""
        checks = {}
        all_passed = True
        
        npc_checks = [
            ('Req_Favor_Ino', 'Ino'),
            ('Req_Favor_Aileen', 'Aileen'),
            ('Req_Favor_Kyle', 'Kyle'),
            ('Req_Favor_Lian', 'Lian')
        ]
        
        for req_col, npc_name in npc_checks:
            if req_col in ending and pd.notna(ending[req_col]):
                operator, required = self.parse_condition(ending[req_col])
                if operator:
                    current = self.npc_favor.get(npc_name, 0)
                    
                    if operator == '>=':
                        passed = current >= required
                    elif operator == '<=':
                        passed = current <= required
                    else:
                        passed = current == required
                    
                    checks[npc_name] = {
                        'required': f"{operator}{required}",
                        'current': current,
                        'passed': passed
                    }
                    
                    if not passed:
                        all_passed = False
        
        return all_passed, checks
    
    def check_event_condition(self, ending: pd.Series) -> Tuple[bool, Dict]:
        """엔딩 이벤트 조건 체크"""
        checks = {}
        all_passed = True
        
        if 'Req_Events' in ending and pd.notna(ending['Req_Events']):
            required_events = str(ending['Req_Events']).split(';')
            for event in required_events:
                event = event.strip()
                if event:
                    passed = event in self.completed_events
                    checks[event] = {
                        'required': 'completed',
                        'current': 'completed' if passed else 'not completed',
                        'passed': passed
                    }
                    if not passed:
                        all_passed = False
        
        return all_passed, checks
    
    def check_flag_condition(self, ending: pd.Series) -> Tuple[bool, Dict]:
        """엔딩 플래그 조건 체크"""
        checks = {}
        all_passed = True
        
        if 'Req_Flags' in ending and pd.notna(ending['Req_Flags']):
            required_flags = str(ending['Req_Flags']).split(';')
            for flag_cond in required_flags:
                flag_cond = flag_cond.strip()
                if flag_cond:
                    # Flag_Name=TRUE/FALSE 형식
                    if '=' in flag_cond:
                        flag_name, flag_value = flag_cond.split('=')
                        flag_name = flag_name.strip()
                        flag_value = flag_value.strip().upper() == 'TRUE'
                        
                        current = flag_name in self.flags
                        passed = current == flag_value
                        
                        checks[flag_name] = {
                            'required': f"={'TRUE' if flag_value else 'FALSE'}",
                            'current': 'TRUE' if current else 'FALSE',
                            'passed': passed
                        }
                        
                        if not passed:
                            all_passed = False
        
        return all_passed, checks
    
    def check_ending_condition(self, ending: pd.Series) -> Tuple[bool, int, Dict]:
        """엔딩 전체 조건 체크"""
        ending_id = ending['Ending_ID']
        
        # 각 조건 체크
        stat_passed, stat_checks = self.check_stat_condition(ending)
        npc_passed, npc_checks = self.check_npc_condition(ending)
        event_passed, event_checks = self.check_event_condition(ending)
        flag_passed, flag_checks = self.check_flag_condition(ending)
        
        # 전체 통과 여부
        all_passed = stat_passed and npc_passed and event_passed and flag_passed
        
        # 우선순위 (안전하게 파싱)
        try:
            priority = int(ending['Priority']) if 'Priority' in ending and pd.notna(ending['Priority']) else 99
        except (ValueError, TypeError):
            priority = 99
        
        details = {
            'ending_id': ending_id,
            'ending_name': ending.get('Name_KO', ending_id),
            'type': ending.get('Type', 'Unknown'),
            'priority': priority,
            'stat_checks': stat_checks,
            'npc_checks': npc_checks,
            'event_checks': event_checks,
            'flag_checks': flag_checks,
            'all_passed': all_passed
        }
        
        return all_passed, priority, details
    
    def simulate_single_playthrough(self, playthrough_name: str = "플레이스루") -> Dict:
        """단일 플레이스루 시뮬레이션"""
        self.log(f"=== {playthrough_name} 시뮬레이션 시작 ===", "START")
        
        # 게임 진행 시뮬레이션 (144턴)
        for turn in range(self.max_turns):
            self.turn = turn + 1
            
            # 연령 업데이트
            if turn > 0 and turn % 12 == 0:
                self.age += 1
            
            # 랜덤 활동으로 스탯/호감도 변화
            self._apply_random_progression()
            
            # 18세가 되면 엔딩 체크
            if self.age >= 18:
                break
        
        # 엔딩 조건 체크
        available_endings = []
        
        for _, ending in self.endings_df.iterrows():
            passed, priority, details = self.check_ending_condition(ending)
            if passed:
                available_endings.append(details)
        
        # 우선순위별 정렬 (낮을수록 높은 우선순위)
        available_endings.sort(key=lambda x: x['priority'])
        
        # 최종 엔딩 결정
        final_ending = available_endings[0] if available_endings else None
        
        result = {
            'playthrough_name': playthrough_name,
            'final_turn': self.turn,
            'final_age': self.age,
            'final_stats': self.stats.copy(),
            'final_favor': self.npc_favor.copy(),
            'available_endings': available_endings,
            'final_ending': final_ending,
            'completed_events': list(self.completed_events),
            'flags': list(self.flags)
        }
        
        if final_ending:
            self.log(f"엔딩 달성: [{final_ending['ending_id']}] {final_ending['ending_name']} (Priority: {final_ending['priority']})", "SUCCESS")
        else:
            self.log("달성한 엔딩 없음", "WARN")
        
        self.log(f"=== {playthrough_name} 시뮬레이션 완료 ===", "END")
        
        return result
    
    def _apply_random_progression(self):
        """랜덤 게임 진행 (스탯/호감도 변화)"""
        # 스탯 변화 (간소화)
        stat_changes = {
            'HP': random.randint(-5, 15),
            'CHARM': random.randint(-3, 12),
            'INT': random.randint(-3, 12),
            'ART': random.randint(-3, 12),
            'MORALITY': random.randint(-2, 10),
            'STRESS': random.randint(-10, 10)
        }
        
        for stat, change in stat_changes.items():
            max_val = 999 if stat != 'STRESS' else 100
            self.stats[stat] = max(0, min(max_val, self.stats[stat] + change))
        
        # NPC 호감도 변화 (50% 확률로 상호작용)
        if random.random() < 0.5:
            npc = random.choice(list(self.npc_favor.keys()))
            
            # 상호작용 유형별 상승량 (BalanceSheet 기준)
            interaction_type = random.choices(
                ['dialogue', 'gift', 'event'],
                weights=[0.5, 0.2, 0.3]  # 대화 50%, 선물 20%, 이벤트 30%
            )[0]
            
            if interaction_type == 'dialogue':
                favor_change = random.randint(3, 8)  # 대화: +3~8
            elif interaction_type == 'gift':
                favor_change = random.randint(8, 20)  # 선물: +8~20
            else:  # event
                favor_change = random.randint(5, 15)  # 이벤트: +5~15
            
            old_favor = self.npc_favor[npc]
            self.npc_favor[npc] = min(150, self.npc_favor[npc] + favor_change)
            
            # 10턴마다 로깅
            if self.turn % 10 == 0:
                self.log(f"턴 {self.turn}: {npc}와 {interaction_type} - 호감도 {old_favor} -> {self.npc_favor[npc]} (+{favor_change})")
        
        # 이벤트 완료 (10% 확률)
        if random.random() < 0.1:
            event_id = f"EVT_RANDOM_{random.randint(1, 50)}"
            self.completed_events.add(event_id)
        
        # 플래그 설정 (5% 확률)
        if random.random() < 0.05:
            flag = f"Flag_{random.randint(1, 10)}"
            self.flags.add(flag)
    
    def simulate_multiple_playthroughs(self, count: int = 10) -> List[Dict]:
        """여러 번 플레이스루 시뮬레이션"""
        self.log(f"=== {count}회 플레이스루 시뮬레이션 시작 ===", "START")
        
        results = []
        ending_counts = {}
        
        for i in range(count):
            self.reset_game_state()
            result = self.simulate_single_playthrough(f"플레이스루 {i+1}")
            results.append(result)
            
            if result['final_ending']:
                ending_name = result['final_ending']['ending_name']
                ending_counts[ending_name] = ending_counts.get(ending_name, 0) + 1
        
        self.log(f"=== {count}회 플레이스루 시뮬레이션 완료 ===", "END")
        self.log(f"엔딩 달성 통계: {ending_counts}")
        
        return results, ending_counts
    
    def simulate_ending_reachability(self) -> Dict:
        """각 엔딩의 도달 가능성 테스트"""
        self.log("=== 엔딩 도달 가능성 테스트 시작 ===", "START")
        
        results = {}
        
        for _, ending in self.endings_df.iterrows():
            ending_id = ending['Ending_ID']
            ending_name = ending.get('Name_KO', ending_id)
            
            # 필요 조건 분석
            requirements = self._analyze_ending_requirements(ending)
            
            # 테스트: 이 엔딩을 목표로 달성 가능한지
            achievable = self._test_ending_achievable(ending)
            
            results[ending_id] = {
                'name': ending_name,
                'type': ending.get('Type', 'Unknown'),
                'priority': ending.get('Priority', 99),
                'requirements': requirements,
                'achievable': achievable
            }
            
            status = "✅ 가능" if achievable else "❌ 불가능"
            self.log(f"[{ending_id}] {ending_name}: {status}")
        
        self.log("=== 엔딩 도달 가능성 테스트 완료 ===", "END")
        
        return results
    
    def _analyze_ending_requirements(self, ending: pd.Series) -> Dict:
        """엔딩 요구사항 분석"""
        requirements = {
            'stats': {},
            'npc_favor': {},
            'events': [],
            'flags': []
        }
        
        # 스탯 요구사항
        for col, stat_name in [
            ('Req_HP', 'HP'), ('Req_Charm', 'CHARM'), ('Req_Int', 'INT'),
            ('Req_Art', 'ART'), ('Req_Morality', 'MORALITY'), ('Req_Stress', 'STRESS')
        ]:
            if col in ending and pd.notna(ending[col]):
                requirements['stats'][stat_name] = str(ending[col])
        
        # NPC 요구사항
        for col, npc_name in [
            ('Req_Favor_Ino', 'Ino'), ('Req_Favor_Aileen', 'Aileen'),
            ('Req_Favor_Kyle', 'Kyle'), ('Req_Favor_Lian', 'Lian')
        ]:
            if col in ending and pd.notna(ending[col]):
                requirements['npc_favor'][npc_name] = str(ending[col])
        
        # 이벤트 요구사항
        if 'Req_Events' in ending and pd.notna(ending['Req_Events']):
            requirements['events'] = str(ending['Req_Events']).split(';')
        
        # 플래그 요구사항
        if 'Req_Flags' in ending and pd.notna(ending['Req_Flags']):
            requirements['flags'] = str(ending['Req_Flags']).split(';')
        
        return requirements
    
    def _test_ending_achievable(self, ending: pd.Series) -> bool:
        """엔딩 달성 가능 여부 테스트"""
        # 극단적인 시나리오로 테스트
        max_stats = {'HP': 999, 'CHARM': 999, 'INT': 999, 'ART': 999, 'MORALITY': 999, 'STRESS': 0}
        max_favor = {'Ino': 150, 'Aileen': 150, 'Kyle': 150, 'Lian': 150}
        
        # 임시 저장
        orig_stats = self.stats.copy()
        orig_favor = self.npc_favor.copy()
        
        # 최대값 설정
        self.stats = max_stats
        self.npc_favor = max_favor
        
        # 체크
        passed, _, _ = self.check_ending_condition(ending)
        
        # 복원
        self.stats = orig_stats
        self.npc_favor = orig_favor
        
        return passed
    
    def generate_markdown_report(self, output_path: str = None):
        """마크다운 보고서 생성"""
        if output_path is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            output_path = f"C:\\우수민\\Maker\\GameProject\\Ending_Simulation_Report_{timestamp}.md"
        
        self.log("엔딩 시뮬레이션 보고서 생성 시작", "START")
        
        # 1. 엔딩 도달 가능성 테스트
        reachability_results = self.simulate_ending_reachability()
        
        # 2. 여러 번 플레이스루
        playthrough_results, ending_counts = self.simulate_multiple_playthroughs(count=20)
        
        # 3. 상세 플레이스루 1개
        self.reset_game_state()
        detailed_result = self.simulate_single_playthrough("상세 분석")
        
        self.log("엔딩 시뮬레이션 완료", "END")
        
        # 마크다운 생성
        md_content = []
        
        # 헤더
        md_content.append("# 엔딩 분기/달성 시뮬레이션 보고서")
        md_content.append(f"\n**생성 시간:** {datetime.now().strftime('%Y년 %m월 %d일 %H:%M:%S')}")
        md_content.append(f"**데이터 출처:** {self.data_dir}")
        md_content.append(f"**총 엔딩 수:** {len(self.endings_df)}")
        md_content.append("\n---\n")
        
        # 목차
        md_content.append("## 목차")
        md_content.append("1. [엔딩 목록 및 조건](#1-엔딩-목록-및-조건)")
        md_content.append("2. [엔딩 도달 가능성 분석](#2-엔딩-도달-가능성-분석)")
        md_content.append("3. [플레이스루 통계](#3-플레이스루-통계)")
        md_content.append("4. [상세 플레이스루 분석](#4-상세-플레이스루-분석)")
        md_content.append("5. [조건 달성률 분석](#5-조건-달성률-분석)")
        md_content.append("6. [권장사항](#6-권장사항)")
        md_content.append("7. [상세 로그](#7-상세-로그)")
        md_content.append("\n---\n")
        
        # 1. 엔딩 목록
        md_content.append("## 1. 엔딩 목록 및 조건\n")
        
        # 기본/히든 엔딩 분리
        basic_endings = self.endings_df[self.endings_df['Type'] == 'Basic']
        hidden_endings = self.endings_df[self.endings_df['Type'] == 'Hidden']
        
        md_content.append(f"### 1.1 기본 엔딩 ({len(basic_endings)}개)\n")
        md_content.append("| 엔딩 ID | 이름 | 우선순위 | 주요 조건 |")
        md_content.append("|---------|------|----------|-----------|")
        
        for _, ending in basic_endings.iterrows():
            conditions = []
            for col, name in [('Req_HP', 'HP'), ('Req_Charm', '매력'), ('Req_Int', '지능'), ('Req_Art', '예술'), ('Req_Morality', '도덕')]:
                if col in ending and pd.notna(ending[col]):
                    conditions.append(f"{name}{ending[col]}")
            
            cond_str = ', '.join(conditions[:3]) if conditions else '특별 조건 없음'
            md_content.append(f"| {ending['Ending_ID']} | {ending['Name_KO']} | {ending['Priority']} | {cond_str} |")
        
        md_content.append("")
        
        if not hidden_endings.empty:
            md_content.append(f"### 1.2 히든 엔딩 ({len(hidden_endings)}개)\n")
            md_content.append("| 엔딩 ID | 이름 | 우선순위 | 특별 조건 |")
            md_content.append("|---------|------|----------|-----------|")
            
            for _, ending in hidden_endings.iterrows():
                special = []
                if 'Req_Flags' in ending and pd.notna(ending['Req_Flags']):
                    special.append("플래그 필요")
                if 'Req_Events' in ending and pd.notna(ending['Req_Events']):
                    special.append("이벤트 필요")
                
                special_str = ', '.join(special) if special else '숨겨진 조건'
                md_content.append(f"| {ending['Ending_ID']} | {ending['Name_KO']} | {ending['Priority']} | {special_str} |")
            
            md_content.append("")
        
        # 2. 엔딩 도달 가능성
        md_content.append("## 2. 엔딩 도달 가능성 분석\n")
        md_content.append("| 엔딩 ID | 이름 | 타입 | 우선순위 | 달성 가능 |")
        md_content.append("|---------|------|------|----------|----------|")
        
        for ending_id, data in reachability_results.items():
            status = "✅" if data['achievable'] else "❌"
            md_content.append(f"| {ending_id} | {data['name']} | {data['type']} | {data['priority']} | {status} |")
        
        md_content.append("")
        
        # 3. 플레이스루 통계
        md_content.append("## 3. 플레이스루 통계\n")
        md_content.append(f"**총 플레이스루:** 20회\n")
        
        md_content.append("### 3.1 엔딩 달성 통계")
        md_content.append("| 엔딩명 | 달성 횟수 | 비율 |")
        md_content.append("|--------|-----------|------|")
        
        for ending_name, count in sorted(ending_counts.items(), key=lambda x: x[1], reverse=True):
            percentage = (count / 20) * 100
            md_content.append(f"| {ending_name} | {count} | {percentage:.1f}% |")
        
        if not ending_counts:
            md_content.append("| *달성한 엔딩 없음* | - | - |")
        
        md_content.append("")
        
        # 4. 상세 플레이스루
        md_content.append("## 4. 상세 플레이스루 분석\n")
        
        if detailed_result['final_ending']:
            final = detailed_result['final_ending']
            md_content.append(f"### 최종 엔딩: [{final['ending_id']}] {final['ending_name']}")
            md_content.append(f"- **타입:** {final['type']}")
            md_content.append(f"- **우선순위:** {final['priority']}")
            md_content.append(f"- **달성 턴:** {detailed_result['final_turn']}/{self.max_turns}")
            md_content.append(f"- **달성 연령:** {detailed_result['final_age']}세")
            md_content.append("")
            
            md_content.append("### 4.1 최종 스탯")
            md_content.append("| 스탯 | 값 |")
            md_content.append("|------|-----|")
            for stat, value in detailed_result['final_stats'].items():
                md_content.append(f"| {stat} | {value} |")
            md_content.append("")
            
            md_content.append("### 4.2 최종 NPC 호감도")
            md_content.append("| NPC | 호감도 |")
            md_content.append("|-----|--------|")
            for npc, favor in detailed_result['final_favor'].items():
                md_content.append(f"| {npc} | {favor} |")
            md_content.append("")
            
            md_content.append("### 4.3 조건 체크 상세")
            md_content.append("#### 스탯 조건")
            md_content.append("| 스탯 | 필요 조건 | 현재 값 | 통과 |")
            md_content.append("|------|-----------|---------|------|")
            for stat, check in final['stat_checks'].items():
                status = "✅" if check['passed'] else "❌"
                md_content.append(f"| {stat} | {check['required']} | {check['current']} | {status} |")
            md_content.append("")
            
            if final['npc_checks']:
                md_content.append("#### NPC 조건")
                md_content.append("| NPC | 필요 조건 | 현재 값 | 통과 |")
                md_content.append("|-----|-----------|---------|------|")
                for npc, check in final['npc_checks'].items():
                    status = "✅" if check['passed'] else "❌"
                    md_content.append(f"| {npc} | {check['required']} | {check['current']} | {status} |")
                md_content.append("")
        else:
            md_content.append("*이 플레이스루에서는 엔딩을 달성하지 못했습니다.*\n")
        
        # 5. 조건 달성률 분석
        md_content.append("## 5. 조건 달성률 분석\n")
        
        # 모든 엔딩의 조건 수집
        all_conditions = []
        for _, ending in self.endings_df.iterrows():
            _, _, details = self.check_ending_condition(ending)
            
            for stat, check in details['stat_checks'].items():
                all_conditions.append({
                    'type': 'stat',
                    'name': stat,
                    'required': check['required'],
                    'current': check['current'],
                    'passed': check['passed']
                })
            
            for npc, check in details['npc_checks'].items():
                all_conditions.append({
                    'type': 'npc',
                    'name': npc,
                    'required': check['required'],
                    'current': check['current'],
                    'passed': check['passed']
                })
        
        if all_conditions:
            passed_count = sum(1 for c in all_conditions if c['passed'])
            total_count = len(all_conditions)
            rate = (passed_count / total_count) * 100 if total_count > 0 else 0
            
            md_content.append(f"**전체 조건 달성률:** {passed_count}/{total_count} ({rate:.1f}%)\n")
            
            md_content.append("### 5.1 어려운 조건 (미달성)")
            md_content.append("| 타입 | 항목 | 필요 조건 | 현재 값 | 차이 |")
            md_content.append("|------|------|-----------|---------|------|")
            
            failed_conditions = [c for c in all_conditions if not c['passed']]
            failed_conditions.sort(key=lambda x: abs(self._parse_required(x['required']) - x['current']), reverse=True)
            
            for cond in failed_conditions[:10]:
                required_val = self._parse_required(cond['required'])
                diff = required_val - cond['current']
                md_content.append(f"| {cond['type']} | {cond['name']} | {cond['required']} | {cond['current']} | {diff:+,} |")
            
            md_content.append("")
        
        # 6. 권장사항
        md_content.append("## 6. 권장사항\n")
        
        # 도달 불가능한 엔딩 체크
        impossible_endings = [k for k, v in reachability_results.items() if not v['achievable']]
        if impossible_endings:
            md_content.append("### ⚠️ 조정 필요한 엔딩")
            md_content.append(f"다음 엔딩은 현재 조건으로는 달성 불가능합니다: {', '.join(impossible_endings)}")
            md_content.append("- Ending_Conditions.csv에서 조건을 완화하거나,")
            md_content.append("- 게임 진행 중 해당 조건을 달성할 수 있는 경로를 추가하세요.")
            md_content.append("")
        
        md_content.append("### CSV 파일 조정 가이드")
        md_content.append("**Ending_Conditions.csv:**")
        md_content.append("- `Req_*` 컬럼: 필요 스탯/NPC 호감도 조건 (예: '500+', '>=80')")
        md_content.append("- `Priority`: 엔딩 우선순위 (낮을수록 우선)")
        md_content.append("- `Req_Events`: 필요 이벤트 ID (세미콜론으로 구분)")
        md_content.append("- `Req_Flags`: 필요 플래그 (예: 'Flag_Name=TRUE')")
        md_content.append("")
        
        md_content.append("**Character_Stats.csv:**")
        md_content.append("- `Max_Value`: 스탯 최대치 조정")
        md_content.append("- `Initial_Value`: 게임 시작 시 초기 스탯")
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
        
        # 저장
        output_path = Path(output_path)
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(md_content))
        
        print(f"\n{'='*70}")
        print(f"엔딩 시뮬레이션 완료!")
        print(f"보고서 저장 위치: {output_path}")
        print(f"{'='*70}")
        
        return output_path
    
    def _parse_required(self, required_str: str) -> int:
        """필요 조건값 파싱"""
        required_str = str(required_str).strip()
        
        # 숫자만 추출
        match = re.search(r'\d+', required_str)
        if match:
            return int(match.group())
        
        return 0


def run_ending_simulation():
    """엔딩 시뮬레이션 실행"""
    print("="*70)
    print("엔딩 분기/달성 시뮬레이션 시작")
    print("="*70)
    
    sim = EndingSimulator()
    report_path = sim.generate_markdown_report()
    
    print(f"\n보고서가 생성되었습니다: {report_path}")
    
    return sim, report_path


if __name__ == '__main__':
    sim, report_path = run_ending_simulation()
