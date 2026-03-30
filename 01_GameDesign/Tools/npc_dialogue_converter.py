#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
NPC Dialogues Markdown to CSV Converter
Markdown 형식의 NPC 대사 스크립트를 CSV 형식으로 변환하는 도구

사용법:
    python npc_dialogue_converter.py <input_md_file> <output_csv_file>
    
예시:
    python npc_dialogue_converter.py NPC_Dialogues.md NPC_Dialogues_KO.csv
"""

import re
import csv
import sys
from pathlib import Path


class NPCDialogueConverter:
    """NPC 대사 Markdown을 CSV로 변환하는 클래스"""
    
    def __init__(self):
        self.dialogues = []
        self.current_npc = None
        self.current_level = None
        self.current_event_num = None
        self.current_event_id = None
        self.current_event_title = None
        self.sequence_counter = 0
        
    def parse_markdown(self, content):
        """Markdown 내용을 파싱하여 대사 데이터 추출"""
        lines = content.split('\n')
        i = 0
        
        while i < len(lines):
            line = lines[i].strip()
            
            # NPC 이름 파싱 (## 1. 이노 (Ino))
            if line.startswith('## ') and not line.startswith('## 레벨'):
                match = re.match(r'## \d+\.\s*(\S+)\s*\(', line)
                if match:
                    self.current_npc = match.group(1)
                    print(f"NPC 발견: {self.current_npc}")
            
            # 레벨 파싱 (### 레벨 1 (0~20): 첫 만남)
            elif line.startswith('### 레벨'):
                match = re.match(r'### 레벨\s*(\d+)', line)
                if match:
                    self.current_level = int(match.group(1))
                    self.current_event_num = 0
                    print(f"  레벨 {self.current_level}")
            
            # 이벤트 제목 파싱 (#### 이벤트 1: 인사이동 첫날)
            elif line.startswith('#### 이벤트'):
                match = re.match(r'#### 이벤트\s*(\d+):\s*(.+)', line)
                if match:
                    self.current_event_num = int(match.group(1))
                    self.current_event_title = match.group(2).strip()
                    self.current_event_id = f"E_{self.get_npc_code()}_{self.current_level}_{self.current_event_num:03d}"
                    self.sequence_counter = 0
                    print(f"    이벤트 {self.current_event_num}: {self.current_event_title}")
            
            # 대사 파싱 ("대사 내용")
            elif line.startswith('"') and self.current_event_id:
                # 여러 줄에 걸친 대사 처리
                dialogue_text = line.strip('"')
                while i + 1 < len(lines) and not lines[i + 1].strip().startswith('"') and not lines[i + 1].strip().startswith('**'):
                    if lines[i + 1].strip():
                        dialogue_text += " " + lines[i + 1].strip().strip('"')
                    i += 1
                
                self.sequence_counter += 1
                dialogue_id = f"D_{self.get_npc_code()}_{self.current_level}_{self.current_event_num:03d}_{self.sequence_counter:03d}"
                
                self.dialogues.append({
                    'Dialogue_ID': dialogue_id,
                    'Event_ID': self.current_event_id,
                    'NPC_Name': self.current_npc,
                    'Level': self.current_level,
                    'Event_Number': self.current_event_num,
                    'Text_Type': 'Dialogue',
                    'Sequence': self.sequence_counter,
                    'Text_Content': dialogue_text,
                    'Choice_ID': '',
                    'Condition': '',
                    'Target_NPC': '',
                    'Stat_Type': '',
                    'Stat_Change': '',
                    'Flag_Set': '',
                    'Next_Event': ''
                })
            
            # 선택지 파싱 (- 선택지 1: "선택지 텍스트" → 결과)
            elif line.startswith('- 선택지') and self.current_event_id:
                match = re.match(r'- 선택지\s*(\d+):\s*"([^"]+)"', line)
                if match:
                    choice_num = int(match.group(1))
                    choice_text = match.group(2)
                    choice_id = f"C_{self.get_npc_code()}_{self.current_level}_{self.current_event_num:03d}_{choice_num:02d}"
                    
                    self.sequence_counter += 1
                    dialogue_id = f"D_{self.get_npc_code()}_{self.current_level}_{self.current_event_num:03d}_C{choice_num:02d}"
                    
                    self.dialogues.append({
                        'Dialogue_ID': dialogue_id,
                        'Event_ID': self.current_event_id,
                        'NPC_Name': self.current_npc,
                        'Level': self.current_level,
                        'Event_Number': self.current_event_num,
                        'Text_Type': 'Choice',
                        'Sequence': choice_num,
                        'Text_Content': choice_text,
                        'Choice_ID': choice_id,
                        'Condition': '',
                        'Target_NPC': '',
                        'Stat_Type': '',
                        'Stat_Change': '',
                        'Flag_Set': '',
                        'Next_Event': ''
                    })
                    
                    # 결과 파싱
                    result_line = self.find_result_line(lines, i, choice_num)
                    if result_line:
                        self.parse_result(result_line, choice_id)
            
            i += 1
    
    def find_result_line(self, lines, current_idx, choice_num):
        """결과 라인 찾기"""
        for j in range(current_idx + 1, min(current_idx + 10, len(lines))):
            line = lines[j].strip()
            if line.startswith('**결과:**'):
                # 결과 섹션 시작, 다음 라인들에서 선택지 결과 찾기
                for k in range(j + 1, min(j + 20, len(lines))):
                    result = lines[k].strip()
                    if f'선택지 {choice_num}:' in result or f'- 선택지 {choice_num}' in result:
                        return result
        return None
    
    def parse_result(self, result_line, choice_id):
        """결과 라인 파싱하여 스탯 변경 추출"""
        # 예시: "선택지 1: 호감도 +3, 스트레스 -5" 또는 "호감도 +5, 스트레스 -3 (선택지 1)"
        
        # 호감도 추출
        favor_match = re.search(r'(\S+?)\s*호감도\s*([+-]\d+)', result_line)
        if favor_match:
            target_npc = favor_match.group(1) if favor_match.group(1) not in ['', ' '] else self.current_npc
            if target_npc in ['이노', '아이린', '카일', '리안']:
                self.add_result_entry(choice_id, target_npc, 'Favor', int(favor_match.group(2)))
        elif '호감도' in result_line:
            favor_match = re.search(r'호감도\s*([+-]\d+)', result_line)
            if favor_match:
                self.add_result_entry(choice_id, self.current_npc, 'Favor', int(favor_match.group(1)))
        
        # 스트레스 추출
        stress_match = re.search(r'스트레스\s*([+-]\d+)', result_line)
        if stress_match:
            self.add_result_entry(choice_id, '', 'Stress', int(stress_match.group(1)))
        
        # 체력 추출
        health_match = re.search(r'체력\s*([+-]\d+)', result_line)
        if health_match:
            self.add_result_entry(choice_id, '', 'Health', int(health_match.group(1)))
        
        # 도덕성 추출
        morality_match = re.search(r'도덕성\s*([+-]\d+)', result_line)
        if morality_match:
            self.add_result_entry(choice_id, '', 'Morality', int(morality_match.group(1)))
        
        # 매력 추출
        charm_match = re.search(r'매력\s*([+-]\d+)', result_line)
        if charm_match:
            self.add_result_entry(choice_id, '', 'Charm', int(charm_match.group(1)))
        
        # 지능 추출
        intel_match = re.search(r'지능\s*([+-]\d+)', result_line)
        if intel_match:
            self.add_result_entry(choice_id, '', 'Intelligence', int(intel_match.group(1)))
        
        # 유머 추출
        humor_match = re.search(r'유머\s*([+-]\d+)', result_line)
        if humor_match:
            self.add_result_entry(choice_id, '', 'Humor', int(humor_match.group(1)))
        
        # 이벤트 종료
        if '이벤트 종료' in result_line:
            self.add_result_entry(choice_id, '', 'Event_End', '')
        
        # 특별 엔딩/플래그
        if '특별 엔딩' in result_line or '히든 엔딩' in result_line:
            flag_type = 'Special_Ending' if '특별 엔딩' in result_line else 'Hidden_Ending'
            self.add_result_entry(choice_id, '', flag_type, 'Unlock', 'True', '')
    
    def add_result_entry(self, choice_id, target_npc, stat_type, stat_change, flag_set='', next_event=''):
        """결과 엔트리 추가"""
        self.sequence_counter += 1
        dialogue_id = choice_id.replace('C_', 'D_') + f"_R{self.sequence_counter:02d}"
        
        self.dialogues.append({
            'Dialogue_ID': dialogue_id,
            'Event_ID': self.current_event_id,
            'NPC_Name': self.current_npc,
            'Level': self.current_level,
            'Event_Number': self.current_event_num,
            'Text_Type': 'Result',
            'Sequence': self.sequence_counter,
            'Text_Content': '',
            'Choice_ID': choice_id,
            'Condition': '',
            'Target_NPC': target_npc,
            'Stat_Type': stat_type,
            'Stat_Change': stat_change,
            'Flag_Set': flag_set,
            'Next_Event': next_event
        })
    
    def get_npc_code(self):
        """NPC 이름을 코드로 변환"""
        if not self.current_npc:
            return 'Unknown'
        npc_codes = {
            '이노': 'Ino',
            '아이린': 'Aileen',
            '카일': 'Kyle',
            '리안': 'Lian'
        }
        return npc_codes.get(self.current_npc, self.current_npc)
    
    def write_csv(self, output_path):
        """CSV 파일로 작성"""
        fieldnames = [
            'Dialogue_ID', 'Event_ID', 'NPC_Name', 'Level', 'Event_Number',
            'Text_Type', 'Sequence', 'Text_Content', 'Choice_ID', 'Condition',
            'Target_NPC', 'Stat_Type', 'Stat_Change', 'Flag_Set', 'Next_Event'
        ]
        
        with open(output_path, 'w', newline='', encoding='utf-8') as f:
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(self.dialogues)
        
        print(f"\n변환 완료: {len(self.dialogues)}개의 레코드가 {output_path}에 저장되었습니다.")


def main():
    if len(sys.argv) != 3:
        print("사용법: python npc_dialogue_converter.py <input_md_file> <output_csv_file>")
        print("예시: python npc_dialogue_converter.py NPC_Dialogues.md NPC_Dialogues_KO.csv")
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    
    if not Path(input_file).exists():
        print(f"오류: 입력 파일을 찾을 수 없습니다: {input_file}")
        sys.exit(1)
    
    print(f"변환 시작: {input_file} → {output_file}\n")
    
    converter = NPCDialogueConverter()
    
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    converter.parse_markdown(content)
    converter.write_csv(output_file)
    
    print("\n변환이 성공적으로 완료되었습니다!")


if __name__ == '__main__':
    main()
