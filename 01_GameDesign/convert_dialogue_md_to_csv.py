#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
NPC_Dialogues.md를 CSV로 변환하는 스크립트
"""

import re
import csv

def parse_dialogue_md(input_file, output_file):
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    rows = []
    
    # NPC별로 섹션 나누기
    npc_sections = re.split(r'## \d+\. (.+) \(', content)
    
    current_npc = None
    current_level = None
    event_counter = {}
    
    lines = content.split('\n')
    
    for i, line in enumerate(lines):
        line = line.strip()
        
        # NPC 헤더 찾기
        if line.startswith('## '):
            match = re.search(r'## \d+\. (.+) \(', line)
            if match:
                current_npc = match.group(1)
                event_counter[current_npc] = {}
                print(f"Processing NPC: {current_npc}")
        
        # 레벨 헤더 찾기
        elif line.startswith('### 레벨 '):
            match = re.search(r'레벨 (\d+)', line)
            if match:
                current_level = int(match.group(1))
                event_counter[current_npc][current_level] = 0
        
        # 이벤트 헤더 찾기
        elif line.startswith('#### 이벤트 '):
            if current_npc and current_level:
                event_counter[current_npc][current_level] += 1
                event_num = event_counter[current_npc][current_level]
        
        # 대사 찾기 (인용문)
        elif line.startswith('"') and line.endswith('"') and current_npc and current_level:
            event_num = event_counter[current_npc].get(current_level, 1)
            npc_code = current_npc[:3].upper() if current_npc != '이노' else 'Ino'
            if current_npc == '아이린':
                npc_code = 'Aileen'
            elif current_npc == '카일':
                npc_code = 'Kyle'
            elif current_npc == '리안':
                npc_code = 'Lian'
            
            dialogue_id = f"D_{npc_code}_L{current_level}_{event_num:03d}"
            text_content = line[1:-1]  # 따옴표 제거
            
            # 이전 라인 확인해서 선택지인지 대사인지 판단
            prev_lines = lines[max(0, i-3):i]
            is_choice = any('선택지' in pl or '**선택지**' in pl for pl in prev_lines)
            
            if is_choice:
                # 선택지 번호 추출
                choice_match = re.search(r'선택지 (\d+):', '\n'.join(prev_lines))
                if choice_match:
                    choice_text = re.search(r'"(.+?)"', text_content)
                    if choice_text:
                        rows.append([dialogue_id, 'Choice', choice_match.group(1), choice_text.group(1)])
            else:
                # 대사인 경우
                seq = len([r for r in rows if r[0] == dialogue_id and r[1] == 'Dialogue']) + 1
                rows.append([dialogue_id, 'Dialogue', str(seq), text_content])
        
        # 결과 찾기
        elif '→' in line and '호감도' in line and current_npc and current_level:
            event_num = event_counter[current_npc].get(current_level, 1)
            npc_code = current_npc[:3].upper() if current_npc != '이노' else 'Ino'
            if current_npc == '아이린':
                npc_code = 'Aileen'
            elif current_npc == '카일':
                npc_code = 'Kyle'
            elif current_npc == '리안':
                npc_code = 'Lian'
            
            dialogue_id = f"D_{npc_code}_L{current_level}_{event_num:03d}"
            
            # 결과 텍스트 추출
            result_text = line.split('→')[1].strip() if '→' in line else line
            seq = len([r for r in rows if r[0] == dialogue_id and r[1] == 'Result']) + 1
            rows.append([dialogue_id, 'Result', str(seq), result_text])
    
    # CSV로 저장
    with open(output_file, 'w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(['Dialogue_ID', 'Text_Type', 'Sequence', 'Text_Content'])
        for row in rows:
            if len(row) == 4:
                writer.writerow(row)
    
    print(f"Total rows: {len(rows)}")
    return len(rows)

if __name__ == '__main__':
    input_file = r'C:\우수민\Maker\GameProject\01_GameDesign\02_Narrative\NPC_Dialogues.md'
    output_file = r'C:\우수민\Maker\GameProject\01_GameDesign\Data\NPC_Dialogues_KO.csv'
    
    count = parse_dialogue_md(input_file, output_file)
    print(f"Conversion complete! {count} rows written.")
