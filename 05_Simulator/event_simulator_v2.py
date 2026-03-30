#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
게임 이벤트 발생 시뮬레이터
Events.csv 데이터를 기반으로 다양한 시나리오에서 이벤트 발생을 시뮬레이션
- 마크다운 로그 파일 생성 지원
"""

import pandas as pd
import random
from datetime import datetime, timedelta
from pathlib import Path
import json
from typing import Dict, List, Any, Tuple

class EventSimulator:
    def __init__(self, events_csv_path):
        """이벤트 시뮬레이터 초기화"""
        self.events_df = self._load_events(events_csv_path)
        self.current_date = datetime(2024, 1, 1)
        self.age = 6
        self.stats = {
            'HP': 100,
            'Charm': 50,
            'Intelligence': 50,
            'Art': 50,
            'Morality': 50,
            'Stress': 0
        }
        self.npc_favor = {
            'Ino': 0,
            'Aileen': 0,
            'Kyle': 0,
            'Lian': 0
        }
        self.flags = set()
        self.completed_events = set()
        self.event_history = []
        self.simulation_logs = []
        self.scenario_name = "Default"
        
    def _load_events(self, csv_path):
        """CSV 파일 로드 (주석 제거)"""
        df = pd.read_csv(csv_path, comment='#', encoding='utf-8')
        return df
    
    def log(self, message: str, level: str = "INFO"):
        """시뮬레이션 로그 기록"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.simulation_logs.append({
            'timestamp': timestamp,
            'level': level,
            'message': message,
            'turn': len(self.event_history)
        })
    
    def set_date(self, year, month, day):
        """현재 날짜 설정"""
        self.current_date = datetime(year, month, day)
        self.log(f"날짜 설정: {self.current_date.strftime('%Y-%m-%d')}")
        
    def set_age(self, age):
        """현재 연령 설정"""
        self.age = age
        self.log(f"연령 설정: {age}세")
        
    def set_stats(self, **kwargs):
        """스탯 설정"""
        for key, value in kwargs.items():
            if key in self.stats:
                self.stats[key] = value
        self.log(f"스탯 설정: {kwargs}")
                
    def set_npc_favor(self, **kwargs):
        """NPC 호감도 설정"""
        for key, value in kwargs.items():
            if key in self.npc_favor:
                self.npc_favor[key] = value
        self.log(f"NPC 호감도 설정: {kwargs}")
    
    def set_scenario_name(self, name: str):
        """시나리오 이름 설정"""
        self.scenario_name = name
        self.log(f"시나리오: {name}")
    
    def check_age_condition(self, age_min, age_max):
        """연령 조건 체크"""
        if pd.isna(age_min) and pd.isna(age_max):
            return True, "연령 제한 없음"
        if pd.isna(age_min):
            result = self.age <= age_max
            return result, f"연령 {self.age} <= {age_max}"
        if pd.isna(age_max):
            result = self.age >= age_min
            return result, f"연령 {self.age} >= {age_min}"
        result = age_min <= self.age <= age_max
        return result, f"연령 {age_min} <= {self.age} <= {age_max}"
    
    def check_date_condition(self, month, day):
        """날짜 조건 체크"""
        if pd.isna(month):
            return True, "날짜 제한 없음"
        if pd.isna(day):
            result = self.current_date.month == month
            return result, f"월 {self.current_date.month} == {month}"
        result = self.current_date.month == month and self.current_date.day == day
        return result, f"날짜 {self.current_date.month}/{self.current_date.day} == {month}/{day}"
    
    def check_stat_condition(self, condition):
        """스탯 조건 체크"""
        if pd.isna(condition) or condition == '':
            return True, "스탯 조건 없음"
        
        condition = str(condition).strip()
        
        for op in ['>=', '<=', '>', '<', '=']:
            if op in condition:
                parts = condition.split(op)
                if len(parts) == 2:
                    stat_name = parts[0].strip()
                    value = int(parts[1].strip())
                    
                    stat_map = {
                        'HP': 'HP', 'HP': 'HP',
                        'Charm': 'Charm', 'CHM': 'Charm',
                        'Intelligence': 'Intelligence', 'INT': 'Intelligence',
                        'Art': 'Art', 'ART': 'Art',
                        'Morality': 'Morality', 'MOR': 'Morality',
                        'Stress': 'Stress', 'STR': 'Stress'
                    }
                    
                    if stat_name in stat_map:
                        actual_stat = stat_map[stat_name]
                        current_value = self.stats.get(actual_stat, 0)
                        
                        if op == '>=':
                            result = current_value >= value
                            return result, f"{actual_stat} {current_value} >= {value}"
                        elif op == '<=':
                            result = current_value <= value
                            return result, f"{actual_stat} {current_value} <= {value}"
                        elif op == '>':
                            result = current_value > value
                            return result, f"{actual_stat} {current_value} > {value}"
                        elif op == '<':
                            result = current_value < value
                            return result, f"{actual_stat} {current_value} < {value}"
                        elif op == '=':
                            result = current_value == value
                            return result, f"{actual_stat} {current_value} == {value}"
        
        return True, f"조건 파싱 불가: {condition}"
    
    def check_npc_condition(self, condition):
        """NPC 조건 체크"""
        if pd.isna(condition) or condition == '':
            return True, "NPC 조건 없음"
        
        condition = str(condition).strip()
        
        # 세미콜론으로 구분된 다중 조건 처리
        if ';' in condition:
            conditions = condition.split(';')
            results = []
            for cond in conditions:
                result, msg = self._check_single_npc_condition(cond.strip())
                results.append((result, msg))
                if not result:
                    return False, f"다중 조건 중 실패: {msg}"
            return True, "; ".join([r[1] for r in results])
        
        return self._check_single_npc_condition(condition)
    
    def _check_single_npc_condition(self, condition):
        """단일 NPC 조건 체크"""
        if 'Favor_' in condition:
            parts = condition.replace('Favor_', '').split('>=')
            if len(parts) == 2:
                npc_name = parts[0].strip()
                value = int(parts[1].strip())
                current = self.npc_favor.get(npc_name, 0)
                result = current >= value
                return result, f"{npc_name} 호감도 {current} >= {value}"
            
            parts = condition.replace('Favor_', '').split('=')
            if len(parts) == 2:
                npc_name = parts[0].strip()
                value = int(parts[1].strip())
                current = self.npc_favor.get(npc_name, 0)
                result = current == value
                return result, f"{npc_name} 호감도 {current} == {value}"
        
        if 'Flag_' in condition:
            flag_name = condition.strip()
            result = flag_name in self.flags
            return result, f"플래그 {flag_name}: {result}"
        
        return True, f"NPC 조건 파싱 불가: {condition}"
    
    def check_trigger_condition(self, trigger_type, trigger_value):
        """트리거 조건 체크"""
        if pd.isna(trigger_type):
            return True, "트리거 없음"
        
        trigger_type = str(trigger_type).strip()
        
        if trigger_type == 'Age':
            if 'Age=' in str(trigger_value):
                required_age = int(str(trigger_value).split('=')[1])
                result = self.age == required_age
                return result, f"연령 {self.age} == {required_age}"
            return True, "연령 조건 없음"
            
        elif trigger_type == 'Date':
            if 'Date=' in str(trigger_value):
                date_str = str(trigger_value).split('=')[1]
                if len(date_str) == 4:
                    month = int(date_str[:2])
                    day = int(date_str[2:])
                    result = self.current_date.month == month and self.current_date.day == day
                    return result, f"날짜 {self.current_date.month}/{self.current_date.day} == {month}/{day}"
            return True, "날짜 조건 없음"
            
        elif trigger_type == 'Random':
            if 'Random=' in str(trigger_value):
                prob_str = str(trigger_value).split('=')[1].replace('%', '')
                probability = int(prob_str) / 100
                roll = random.random()
                result = roll < probability
                return result, f"랜덤 {roll:.2%} < {probability:.0%}"
            return True, "랜덤 조건 없음"
            
        elif trigger_type == 'Favor':
            return self.check_npc_condition(trigger_value)
        
        return True, f"트리거 타입: {trigger_type}"
    
    def can_trigger_event(self, event_row) -> Tuple[bool, str, Dict]:
        """이벤트 발생 가능 여부 체크 및 상세 결과 반환"""
        event_id = event_row['Event_ID']
        checks = {}
        
        # 이미 완료된 이벤트
        if event_id in self.completed_events:
            return False, "Already completed", checks
        
        # 반복 가능 여부
        is_repeatable = event_row.get('Is_Repeatable', False)
        if not pd.isna(is_repeatable) and str(is_repeatable).upper() == 'TRUE':
            pass
        elif event_id in self.completed_events:
            return False, "Not repeatable", checks
        
        # 연령 체크
        age_min = event_row.get('Age_Min')
        age_max = event_row.get('Age_Max')
        age_ok, age_msg = self.check_age_condition(age_min, age_max)
        checks['age'] = age_msg
        if not age_ok:
            return False, f"Age condition failed", checks
        
        # 날짜 체크
        month = event_row.get('Month')
        day = event_row.get('Day')
        date_ok, date_msg = self.check_date_condition(month, day)
        checks['date'] = date_msg
        if not date_ok:
            return False, "Date condition failed", checks
        
        # 스탯 체크
        stat_condition = event_row.get('Stat_Condition')
        stat_ok, stat_msg = self.check_stat_condition(stat_condition)
        checks['stat'] = stat_msg
        if not stat_ok:
            return False, "Stat condition failed", checks
        
        # NPC 조건 체크
        npc_condition = event_row.get('NPC_Condition')
        npc_ok, npc_msg = self.check_npc_condition(npc_condition)
        checks['npc'] = npc_msg
        if not npc_ok:
            return False, "NPC condition failed", checks
        
        # 트리거 체크
        trigger_type = event_row.get('Trigger_Type')
        trigger_value = event_row.get('Trigger_Value')
        trigger_ok, trigger_msg = self.check_trigger_condition(trigger_type, trigger_value)
        checks['trigger'] = trigger_msg
        if not trigger_ok:
            return False, "Trigger condition failed", checks
        
        # 이전 이벤트 체크
        prev_event = event_row.get('Required_Previous_Event')
        if not pd.isna(prev_event) and prev_event != '':
            if prev_event not in self.completed_events:
                checks['previous'] = f"이전 이벤트 미완료: {prev_event}"
                return False, f"Previous event not completed", checks
            checks['previous'] = f"이전 이벤트 완료: {prev_event}"
        
        return True, "All conditions met", checks
    
    def simulate_turn(self) -> List[Dict]:
        """한 턴 시뮬레이션"""
        self.log(f"=== 턴 시작: {self.current_date.strftime('%Y-%m-%d')}, {self.age}세 ===", "TURN")
        
        triggered_events = []
        failed_events = []
        
        for _, event in self.events_df.iterrows():
            can_trigger, reason, checks = self.can_trigger_event(event)
            if can_trigger:
                triggered_events.append({
                    'event': event,
                    'checks': checks
                })
            else:
                if reason != "Already completed" and reason != "Not repeatable":
                    failed_events.append({
                        'event_id': event['Event_ID'],
                        'name': event['Name_KO'],
                        'reason': reason,
                        'checks': checks
                    })
        
        # 우선순위 정렬
        triggered_events.sort(
            key=lambda x: x['event'].get('Priority', 0) if not pd.isna(x['event'].get('Priority')) else 0,
            reverse=True
        )
        
        self.log(f"발생 가능 이벤트: {len(triggered_events)}개", "EVENT")
        
        if triggered_events:
            for i, item in enumerate(triggered_events[:5], 1):
                event = item['event']
                self.log(f"  {i}. [{event['Event_ID']}] {event['Name_KO']} (Priority: {event.get('Priority', 'N/A')})")
            
            # 가장 우선순위 높은 이벤트 발생
            top_event = triggered_events[0]
            self.trigger_event(top_event['event'], top_event['checks'])
        else:
            self.log("발생 가능한 이벤트 없음", "WARN")
        
        return triggered_events, failed_events
    
    def trigger_event(self, event, checks):
        """이벤트 발생 처리"""
        event_id = event['Event_ID']
        self.completed_events.add(event_id)
        
        self.event_history.append({
            'date': self.current_date.strftime('%Y-%m-%d'),
            'age': self.age,
            'event_id': event_id,
            'event_name': event['Name_KO'],
            'type': event['Type'],
            'checks': checks
        })
        
        self.log(f"이벤트 발생: [{event_id}] {event['Name_KO']}", "SUCCESS")
        
        next_event = event.get('Next_Event')
        if not pd.isna(next_event) and next_event != '':
            self.log(f"  → 다음 이벤트 예약: {next_event}")
    
    def advance_time(self, months=1):
        """시간 진행"""
        self.current_date += timedelta(days=30*months)
        
        if self.current_date.month == 12 and self.current_date.day == 24:
            self.age += 1
            self.log(f"생일! 루아가 {self.age}세가 되었습니다!", "EVENT")
    
    def run_full_simulation(self, turns=12):
        """전체 시뮬레이션 실행"""
        self.log(f"시뮬레이션 시작: {turns}턴", "START")
        
        for turn in range(turns):
            self.log(f"턴 {turn + 1}/{turns}", "TURN")
            self.simulate_turn()
            self.advance_time(1)
        
        self.log("시뮬레이션 완료", "END")
    
    def generate_markdown_report(self, output_path: str = None):
        """마크다운 형식의 시뮬레이션 결과 보고서 생성"""
        if output_path is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            scenario_name = self.scenario_name.replace(" ", "_")
            output_path = f"C:\\우수민\\Maker\\GameProject\\Event_Simulation_{scenario_name}_{timestamp}.md"
        
        md_content = []
        
        # 헤더
        md_content.append(f"# 이벤트 발생 시뮬레이션 결과 보고서")
        md_content.append(f"\n**시나리오:** {self.scenario_name}")
        md_content.append(f"**생성 시간:** {datetime.now().strftime('%Y년 %m월 %d일 %H:%M:%S')}")
        md_content.append(f"**시뮬레이션 턴 수:** {len(self.event_history)}")
        md_content.append("\n---\n")
        
        # 1. 최종 상태 요약
        md_content.append("## 1. 최종 상태 요약\n")
        md_content.append(f"- **최종 연령:** {self.age}세")
        md_content.append(f"- **최종 날짜:** {self.current_date.strftime('%Y년 %m월 %d일')}")
        md_content.append(f"- **총 발생 이벤트:** {len(self.event_history)}개")
        md_content.append(f"- **완료된 이벤트:** {len(self.completed_events)}개\n")
        
        # 2. 시뮬레이션 설정
        md_content.append("## 2. 시뮬레이션 설정\n")
        md_content.append("### 초기 스탯")
        md_content.append("| 스탯 | 값 |")
        md_content.append("|------|-----|")
        for stat, value in self.stats.items():
            md_content.append(f"| {stat} | {value} |")
        md_content.append("")
        
        md_content.append("### 초기 NPC 호감도")
        md_content.append("| NPC | 호감도 |")
        md_content.append("|-----|--------|")
        for npc, favor in self.npc_favor.items():
            md_content.append(f"| {npc} | {favor} |")
        md_content.append("")
        
        # 3. 이벤트 발생 이력
        md_content.append("## 3. 이벤트 발생 이력\n")
        if self.event_history:
            md_content.append("| 턴 | 날짜 | 연령 | 이벤트 ID | 이벤트명 | 타입 | 우선순위 |")
            md_content.append("|-----|------|------|-----------|----------|------|----------|")
            for i, event in enumerate(self.event_history, 1):
                event_data = self.events_df[self.events_df['Event_ID'] == event['event_id']].iloc[0] if len(self.events_df[self.events_df['Event_ID'] == event['event_id']]) > 0 else None
                priority = event_data['Priority'] if event_data is not None else 'N/A'
                md_content.append(f"| {i} | {event['date']} | {event['age']}세 | {event['event_id']} | {event['event_name']} | {event['type']} | {priority} |")
        else:
            md_content.append("*발생한 이벤트 없음*")
        md_content.append("")
        
        # 4. 이벤트 타입별 통계
        md_content.append("## 4. 이벤트 타입별 통계\n")
        event_types = {}
        for event in self.event_history:
            event_type = event['type']
            event_types[event_type] = event_types.get(event_type, 0) + 1
        
        if event_types:
            md_content.append("| 이벤트 타입 | 발생 횟수 | 비율 |")
            md_content.append("|-------------|-----------|------|")
            total = len(self.event_history)
            for event_type, count in sorted(event_types.items(), key=lambda x: x[1], reverse=True):
                percentage = (count / total * 100) if total > 0 else 0
                md_content.append(f"| {event_type} | {count} | {percentage:.1f}% |")
        md_content.append("")
        
        # 5. 상세 이벤트 발생 조건 체크
        md_content.append("## 5. 이벤트 발생 조건 상세 분석\n")
        for event in self.event_history:
            md_content.append(f"### {event['event_id']} - {event['event_name']}")
            md_content.append(f"- **발생 시기:** {event['date']} ({event['age']}세)")
            md_content.append(f"- **타입:** {event['type']}")
            md_content.append("- **통과한 조건 체크:**")
            for check_type, check_msg in event.get('checks', {}).items():
                md_content.append(f"  - {check_type}: {check_msg}")
            md_content.append("")
        
        # 6. 시뮬레이션 로그
        md_content.append("## 6. 시뮬레이션 전체 로그\n")
        md_content.append("<details>")
        md_content.append("<summary>전체 로그 보기 (클릭)</summary>")
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
        
        self.log(f"보고서 저장 완료: {output_path}")
        return output_path


def run_scenario_tests_with_reports():
    """모든 시나리오 테스트 실행 및 마크다운 보고서 생성"""
    events_path = Path("C:\\우수민\\Maker\\GameProject\\01_GameDesign\\Data\\Events.csv")
    reports = []
    
    scenarios = [
        {
            'name': '6세 기본 진행',
            'age': 6,
            'date': (2024, 1, 1),
            'turns': 12,
            'stats': {},
            'npc_favor': {}
        },
        {
            'name': '15세 고연령 테스트',
            'age': 15,
            'date': (2024, 3, 1),
            'turns': 6,
            'stats': {},
            'npc_favor': {}
        },
        {
            'name': '생일 이벤트 테스트',
            'age': 10,
            'date': (2024, 12, 24),
            'turns': 1,
            'stats': {},
            'npc_favor': {}
        },
        {
            'name': '스트레스 이벤트 테스트',
            'age': 12,
            'date': (2024, 6, 1),
            'turns': 1,
            'stats': {'Stress': 60},
            'npc_favor': {}
        },
        {
            'name': 'NPC 호감도 이벤트 테스트',
            'age': 16,
            'date': (2024, 6, 1),
            'turns': 1,
            'stats': {},
            'npc_favor': {'Lian': 85, 'Ino': 75}
        },
        {
            'name': '히든 엔딩 조건 테스트',
            'age': 18,
            'date': (2024, 12, 24),
            'turns': 1,
            'stats': {'Intelligence': 900, 'Art': 900},
            'npc_favor': {'Lian': 100, 'Ino': 100}
        }
    ]
    
    print("="*70)
    print("모든 시나리오 시뮬레이션 실행")
    print("="*70)
    
    for i, scenario in enumerate(scenarios, 1):
        print(f"\n{'='*70}")
        print(f"시나리오 {i}: {scenario['name']}")
        print(f"{'='*70}")
        
        sim = EventSimulator(events_path)
        sim.set_scenario_name(scenario['name'])
        sim.set_age(scenario['age'])
        sim.set_date(*scenario['date'])
        
        if scenario['stats']:
            sim.set_stats(**scenario['stats'])
        if scenario['npc_favor']:
            sim.set_npc_favor(**scenario['npc_favor'])
        
        sim.run_full_simulation(turns=scenario['turns'])
        report_path = sim.generate_markdown_report()
        reports.append(report_path)
        
        print(f"\n보고서 저장: {report_path}")
        print(f"발생 이벤트: {len(sim.event_history)}개")
    
    # 통합 보고서 생성
    print(f"\n{'='*70}")
    print("통합 보고서 생성")
    print(f"{'='*70}")
    
    combined_md = []
    combined_md.append("# 이벤트 발생 시뮬레이션 통합 보고서")
    combined_md.append(f"\n**생성 시간:** {datetime.now().strftime('%Y년 %m월 %d일 %H:%M:%S')}")
    combined_md.append(f"**총 시나리오:** {len(scenarios)}개")
    combined_md.append("\n---\n")
    
    combined_md.append("## 목차")
    for i, scenario in enumerate(scenarios, 1):
        combined_md.append(f"{i}. [{scenario['name']}](#{i}-{scenario['name'].replace(' ', '-')})")
    combined_md.append("")
    
    for i, report_path in enumerate(reports, 1):
        with open(report_path, 'r', encoding='utf-8') as f:
            content = f.read()
            # 첫 줄(제목) 제외하고 내용만 추가
            lines = content.split('\n')[1:]
            combined_md.extend(lines)
            combined_md.append("\n---\n")
    
    combined_path = f"C:\\우수민\\Maker\\GameProject\\Event_Simulation_Combined_{datetime.now().strftime('%Y%m%d_%H%M%S')}.md"
    with open(combined_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(combined_md))
    
    print(f"\n통합 보고서 저장: {combined_path}")
    print(f"\n{'='*70}")
    print("모든 시뮬레이션 완료!")
    print(f"{'='*70}")
    
    return reports, combined_path


if __name__ == '__main__':
    reports, combined = run_scenario_tests_with_reports()
