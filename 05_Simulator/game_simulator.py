#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
게임 통합 시뮬레이터
- 이벤트 발생 시뮬레이션
- NPC 호감도 시뮬레이션
- 결과 로깅 (Markdown 파일)
"""

import pandas as pd
import random
from datetime import datetime, timedelta
from pathlib import Path
import json
from typing import Dict, List, Any

class GameSimulator:
    def __init__(self, events_csv_path, npc_dialogues_path=None):
        """게임 시뮬레이터 초기화"""
        self.events_df = self._load_events(events_csv_path)
        self.npc_dialogues_df = self._load_npc_dialogues(npc_dialogues_path) if npc_dialogues_path else None
        
        # 게임 상태
        self.current_date = datetime(2024, 1, 1)
        self.age = 6
        self.stats = {
            'HP': 100, 'Charm': 50, 'Intelligence': 50, 
            'Art': 50, 'Morality': 50, 'Stress': 0
        }
        self.npc_favor = {
            'Ino': 20, 'Aileen': 20, 'Kyle': 20, 'Lian': 0
        }
        self.flags = set()
        self.completed_events = set()
        
        # 로그 데이터
        self.event_history = []
        self.npc_interactions = []
        self.stat_changes = []
        self.simulation_logs = []
        
    def _load_events(self, csv_path):
        """이벤트 CSV 로드"""
        df = pd.read_csv(csv_path, comment='#', encoding='utf-8')
        return df
    
    def _load_npc_dialogues(self, csv_path):
        """NPC 대사 CSV 로드"""
        try:
            df = pd.read_csv(csv_path, encoding='utf-8')
            return df
        except:
            return None
    
    def log(self, message: str, level: str = "INFO"):
        """시뮬레이션 로그 기록"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.simulation_logs.append({
            'timestamp': timestamp,
            'level': level,
            'message': message,
            'turn': len(self.event_history)
        })
    
    def simulate_npc_interaction(self, npc_name: str, interaction_type: str = "dialogue"):
        """NPC 상호작용 시뮬레이션"""
        self.log(f"NPC 상호작용 시작: {npc_name} ({interaction_type})")
        
        current_favor = self.npc_favor.get(npc_name, 0)
        favor_change = 0
        
        if interaction_type == "dialogue":
            # 대화 시 기본 호감도 상승
            favor_change = random.randint(3, 8)
        elif interaction_type == "gift":
            # 선물 시 추가 상승
            favor_change = random.randint(8, 20)
        elif interaction_type == "event":
            # 이벤트 선택지
            favor_change = random.randint(5, 15)
        
        old_favor = self.npc_favor[npc_name]
        self.npc_favor[npc_name] = min(100, current_favor + favor_change)
        actual_change = self.npc_favor[npc_name] - old_favor
        
        # 레벨 계산
        old_level = self._get_favor_level(old_favor)
        new_level = self._get_favor_level(self.npc_favor[npc_name])
        
        interaction_record = {
            'date': self.current_date.strftime('%Y-%m-%d'),
            'age': self.age,
            'npc_name': npc_name,
            'interaction_type': interaction_type,
            'favor_before': old_favor,
            'favor_after': self.npc_favor[npc_name],
            'favor_change': actual_change,
            'level_before': old_level,
            'level_after': new_level,
            'level_up': new_level > old_level
        }
        
        self.npc_interactions.append(interaction_record)
        
        self.log(f"NPC {npc_name}: 호감도 {old_favor} → {self.npc_favor[npc_name]} (+{actual_change})")
        if new_level > old_level:
            self.log(f"NPC {npc_name}: 레벨 업! {old_level} → {new_level}", "EVENT")
        
        return interaction_record
    
    def _get_favor_level(self, favor: int) -> int:
        """호감도에 따른 레벨 계산"""
        if favor >= 80: return 5
        elif favor >= 60: return 4
        elif favor >= 40: return 3
        elif favor >= 20: return 2
        else: return 1
    
    def simulate_activity(self, activity_type: str):
        """활동 시뮬레이션 및 스탯 변경"""
        self.log(f"활동 실행: {activity_type}")
        
        stat_changes = {}
        
        if activity_type == "study":
            stat_changes = {'Intelligence': random.randint(5, 12), 'Stress': random.randint(3, 8)}
        elif activity_type == "charm_training":
            stat_changes = {'Charm': random.randint(5, 12), 'Stress': random.randint(2, 6)}
        elif activity_type == "exercise":
            stat_changes = {'HP': random.randint(8, 15), 'Stress': random.randint(5, 10)}
        elif activity_type == "art":
            stat_changes = {'Art': random.randint(5, 12), 'Stress': random.randint(2, 6)}
        elif activity_type == "morality":
            stat_changes = {'Morality': random.randint(3, 8), 'Stress': random.randint(-5, -2)}
        elif activity_type == "rest":
            stat_changes = {'Stress': random.randint(-15, -8), 'HP': random.randint(3, 8)}
        elif activity_type == "part_time":
            stat_changes = {'Stress': random.randint(5, 12)}
        
        # 스탯 적용
        for stat, change in stat_changes.items():
            if stat in self.stats:
                old_value = self.stats[stat]
                self.stats[stat] = max(0, min(999, self.stats[stat] + change))
                actual_change = self.stats[stat] - old_value
                
                self.stat_changes.append({
                    'date': self.current_date.strftime('%Y-%m-%d'),
                    'age': self.age,
                    'activity': activity_type,
                    'stat_name': stat,
                    'old_value': old_value,
                    'new_value': self.stats[stat],
                    'change': actual_change
                })
        
        self.log(f"활동 완료: {activity_type}, 스탯 변경: {stat_changes}")
        return stat_changes
    
    def check_event_conditions(self, event_row):
        """이벤트 발생 조건 체크 (간소화)"""
        event_id = event_row['Event_ID']
        if event_id in self.completed_events:
            return False
        
        # 연령 체크
        age_min = event_row.get('Age_Min')
        age_max = event_row.get('Age_Max')
        if not pd.isna(age_min) and self.age < age_min:
            return False
        if not pd.isna(age_max) and self.age > age_max:
            return False
        
        return True
    
    def simulate_turn(self):
        """한 턴 시뮬레이션"""
        self.log(f"=== 턴 시작: {self.current_date.strftime('%Y-%m-%d')}, {self.age}세 ===")
        
        # 1. 이벤트 체크
        available_events = []
        for _, event in self.events_df.iterrows():
            if self.check_event_conditions(event):
                available_events.append(event)
        
        # 우선순위별 정렬
        available_events.sort(key=lambda x: x.get('Priority', 0) if not pd.isna(x.get('Priority')) else 0, reverse=True)
        
        # 2. 이벤트 발생
        if available_events:
            top_event = available_events[0]
            self.trigger_event(top_event)
        
        # 3. NPC 상호작용 (50% 확률)
        if random.random() < 0.5:
            npc = random.choice(list(self.npc_favor.keys()))
            interaction_type = random.choice(['dialogue', 'dialogue', 'gift', 'event'])
            self.simulate_npc_interaction(npc, interaction_type)
        
        # 4. 활동 실행
        activity = random.choice(['study', 'charm_training', 'exercise', 'art', 'rest', 'part_time'])
        self.simulate_activity(activity)
        
        # 5. 시간 진행
        self.advance_time()
        
    def trigger_event(self, event):
        """이벤트 발생"""
        event_id = event['Event_ID']
        self.completed_events.add(event_id)
        
        self.event_history.append({
            'date': self.current_date.strftime('%Y-%m-%d'),
            'age': self.age,
            'event_id': event_id,
            'event_name': event['Name_KO'],
            'type': event['Type']
        })
        
        self.log(f"이벤트 발생: [{event_id}] {event['Name_KO']}", "EVENT")
    
    def advance_time(self, months=1):
        """시간 진행"""
        self.current_date += timedelta(days=30*months)
        
        # 생일 체크 (12월 24일)
        if self.current_date.month == 12 and self.current_date.day >= 24:
            self.age += 1
            self.log(f"생일! 루아가 {self.age}세가 되었습니다!", "EVENT")
    
    def run_simulation(self, turns=24):
        """전체 시뮬레이션 실행"""
        self.log(f"시뮬레이션 시작: 총 {turns}턴")
        
        for turn in range(turns):
            self.log(f"턴 {turn + 1}/{turns}")
            self.simulate_turn()
        
        self.log("시뮬레이션 완료")
    
    def generate_markdown_report(self, output_path: str = None):
        """마크다운 형식의 시뮬레이션 결과 보고서 생성"""
        if output_path is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            output_path = f"C:\\우수민\\Maker\\GameProject\\Simulation_Report_{timestamp}.md"
        
        md_content = []
        
        # 헤더
        md_content.append("# 게임 시뮬레이션 결과 보고서")
        md_content.append(f"\n**생성 시간:** {datetime.now().strftime('%Y년 %m월 %d일 %H:%M:%S')}")
        md_content.append(f"**시뮬레이션 턴 수:** {len(self.event_history)}")
        md_content.append("\n---\n")
        
        # 1. 최종 상태 요약
        md_content.append("## 1. 최종 상태 요약\n")
        md_content.append(f"- **최종 연령:** {self.age}세")
        md_content.append(f"- **최종 날짜:** {self.current_date.strftime('%Y년 %m월 %d일')}")
        md_content.append(f"- **총 발생 이벤트:** {len(self.event_history)}개")
        md_content.append(f"- **총 NPC 상호작용:** {len(self.npc_interactions)}회")
        md_content.append(f"- **총 스탯 변경:** {len(self.stat_changes)}회\n")
        
        # 2. 최종 스탯
        md_content.append("## 2. 최종 스탯\n")
        md_content.append("| 스탯 | 최종 값 |")
        md_content.append("|------|---------|")
        for stat, value in self.stats.items():
            md_content.append(f"| {stat} | {value} |")
        md_content.append("")
        
        # 3. NPC 호감도 변화
        md_content.append("## 3. NPC 호감도 변화\n")
        md_content.append("| NPC | 최종 호감도 | 최종 레벨 |")
        md_content.append("|-----|-------------|-----------|")
        for npc, favor in self.npc_favor.items():
            level = self._get_favor_level(favor)
            md_content.append(f"| {npc} | {favor} | Level {level} |")
        md_content.append("")
        
        # 4. 이벤트 발생 이력
        md_content.append("## 4. 이벤트 발생 이력\n")
        if self.event_history:
            md_content.append("| 날짜 | 연령 | 이벤트명 | 타입 |")
            md_content.append("|------|------|----------|------|")
            for event in self.event_history:
                md_content.append(f"| {event['date']} | {event['age']}세 | {event['event_name']} | {event['type']} |")
        else:
            md_content.append("*발생한 이벤트 없음*")
        md_content.append("")
        
        # 5. NPC 상호작용 상세
        md_content.append("## 5. NPC 상호작용 상세\n")
        if self.npc_interactions:
            md_content.append("| 날짜 | NPC | 타입 | 호감도 변화 | 레벨 업 |")
            md_content.append("|------|-----|------|-------------|---------|")
            for interaction in self.npc_interactions:
                level_up = "✓" if interaction['level_up'] else ""
                md_content.append(f"| {interaction['date']} | {interaction['npc_name']} | {interaction['interaction_type']} | "
                                f"{interaction['favor_before']} → {interaction['favor_after']} (+{interaction['favor_change']}) | {level_up} |")
        else:
            md_content.append("*NPC 상호작용 없음*")
        md_content.append("")
        
        # 6. NPC 호감도 그래프 (텍스트 형태)
        md_content.append("## 6. NPC 호감도 변화 그래프\n")
        for npc in self.npc_favor.keys():
            npc_data = [i for i in self.npc_interactions if i['npc_name'] == npc]
            if npc_data:
                md_content.append(f"### {npc}")
                md_content.append("```")
                md_content.append(f"호감도: {npc_data[0]['favor_before']} → {npc_data[-1]['favor_after']}")
                md_content.append(f"상호작용 횟수: {len(npc_data)}회")
                md_content.append(f"최종 레벨: {self._get_favor_level(self.npc_favor[npc])}")
                md_content.append("```\n")
        
        # 7. 스탯 변화 추이
        md_content.append("## 7. 스탯 변화 추이\n")
        if self.stat_changes:
            md_content.append("| 날짜 | 활동 | 스탯 | 변화 |")
            md_content.append("|------|------|------|------|")
            for change in self.stat_changes[:30]:  # 최근 30개만
                sign = "+" if change['change'] > 0 else ""
                md_content.append(f"| {change['date']} | {change['activity']} | {change['stat_name']} | {sign}{change['change']} |")
            if len(self.stat_changes) > 30:
                md_content.append(f"*... 외 {len(self.stat_changes) - 30}개 변화*")
        else:
            md_content.append("*스탯 변화 없음*")
        md_content.append("")
        
        # 8. 시뮬레이션 로그
        md_content.append("## 8. 시뮬레이션 로그\n")
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


def run_full_simulation():
    """전체 시뮬레이션 실행 및 결과 생성"""
    events_path = Path("C:\\우수민\\Maker\\GameProject\\01_GameDesign\\Data\\Events.csv")
    
    print("="*70)
    print("게임 통합 시뮬레이션 시작")
    print("="*70)
    
    # 시뮬레이션 생성 및 실행
    sim = GameSimulator(events_path)
    sim.run_simulation(turns=24)  # 24턴 (2년) 시뮬레이션
    
    # 마크다운 보고서 생성
    report_path = sim.generate_markdown_report()
    
    print("\n" + "="*70)
    print("시뮬레이션 완료!")
    print(f"보고서 저장 위치: {report_path}")
    print("="*70)
    
    # 콘솔 요약 출력
    print(f"\n요약:")
    print(f"  - 총 이벤트: {len(sim.event_history)}개")
    print(f"  - 총 NPC 상호작용: {len(sim.npc_interactions)}회")
    print(f"  - 최종 연령: {sim.age}세")
    print(f"\nNPC 호감도:")
    for npc, favor in sim.npc_favor.items():
        level = sim._get_favor_level(favor)
        print(f"  - {npc}: {favor} (Level {level})")
    print(f"\n최종 스탯:")
    for stat, value in sim.stats.items():
        print(f"  - {stat}: {value}")
    
    return sim, report_path


if __name__ == '__main__':
    sim, report_path = run_full_simulation()
