#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Game Data CSV to Excel 변환 스크립트
01_GameDesign/Data 폴더의 모든 CSV 파일을 하나의 Excel 파일로 통합
"""

import pandas as pd
import os
import sys
from pathlib import Path

print("=" * 70)
print("게임 데이터 CSV to Excel 변환 시작")
print("=" * 70)

# 데이터 폴더 경로
data_dir = Path("C:\\우수민\\Maker\\GameProject\\01_GameDesign\\Data")
output_dir = Path("C:\\우수민\\Maker\\GameProject\\01_GameDesign\\Data")

print(f"\n데이터 폴더: {data_dir}")

# 변환할 CSV 파일 목록 (DataSchema 순서대로)
csv_files = {
    'Character_Stats': 'Character_Stats.csv',
    'Characters': 'Characters.csv',
    'Items_Master': 'Items_Master.csv',
    'Equipment': 'Equipment.csv',
    'Actions': 'Actions.csv',
    'Locations': 'Locations.csv',
    'Events': 'Events.csv',
    'Quests': 'Quests.csv',
    'NPC_Favor': 'NPC_Favor.csv',
    'Ending_Conditions': 'Ending_Conditions.csv',
    'CG_Master': 'CG_Master.csv',
    'BGM_Master': 'BGM_Master.csv',
    'NPC_Dialogues_Events': 'NPC_Dialogues_Events.csv',
    'NPC_Dialogues_KO': 'NPC_Dialogues_KO.csv',
    'NPC_Dialogues_EN': 'NPC_Dialogues_EN.csv'
}

# 출력 Excel 파일명
output_file = output_dir / 'GameData.xlsx'

print(f"출력 파일: {output_file}")
print()

def read_csv_with_comments(csv_file):
    """
    주석이 포함된 CSV 파일을 읽는 함수
    #으로 시작하는 줄은 주석으로 처리
    """
    try:
        # 먼저 UTF-8로 시도
        with open(csv_file, 'r', encoding='utf-8') as f:
            lines = f.readlines()
    except UnicodeDecodeError:
        # UTF-8 실패시 cp949로 시도
        print(f"  UTF-8 실패, cp949로 재시도...")
        with open(csv_file, 'r', encoding='cp949') as f:
            lines = f.readlines()
    except Exception as e:
        print(f"  파일 읽기 오류: {e}")
        return None
    
    # 주석 및 빈 줄 제거
    clean_lines = []
    for line in lines:
        stripped = line.strip()
        # 비어있는 줄이나 주석(#으로 시작)이 아닌 줄만 추가
        if stripped and not stripped.startswith('#'):
            clean_lines.append(line)
    
    if not clean_lines:
        print(f"  경고: 데이터가 없습니다.")
        return None
    
    # 임시 파일로 저장 후 pandas로 읽기
    import tempfile
    with tempfile.NamedTemporaryFile(mode='w', encoding='utf-8', suffix='.csv', delete=False) as tmp:
        tmp.writelines(clean_lines)
        tmp_path = tmp.name
    
    try:
        df = pd.read_csv(tmp_path, encoding='utf-8', on_bad_lines='skip', index_col=False)
    except Exception as e:
        print(f"  CSV 파싱 오류: {e}")
        os.unlink(tmp_path)
        return None
    finally:
        try:
            os.unlink(tmp_path)  # 임시 파일 삭제
        except:
            pass
    
    return df

# CSV 파일 존재 확인
missing_files = []
for sheet_name, csv_file in csv_files.items():
    file_path = data_dir / csv_file
    if not file_path.exists():
        missing_files.append(csv_file)

if missing_files:
    print(f"[경고] 다음 파일을 찾을 수 없습니다:")
    for f in missing_files:
        print(f"  - {f}")
    print("\n파일을 확인하고 다시 실행해주세요.")
    sys.exit(1)

# Excel 파일 생성
success_sheets = []
try:
    with pd.ExcelWriter(output_file, engine='openpyxl') as writer:
        for sheet_name, csv_file in csv_files.items():
            try:
                print(f"\n처리 중: {csv_file}...")
                
                file_path = data_dir / csv_file
                
                # 주석 처리 기능으로 CSV 읽기
                df = read_csv_with_comments(file_path)
                
                if df is None or df.empty:
                    print(f"  데이터가 없습니다.")
                    continue
                
                # Excel 시트로 저장
                df.to_excel(writer, sheet_name=sheet_name, index=False)
                success_sheets.append(sheet_name)
                
                print(f"  완료: {sheet_name}")
                print(f"     행: {len(df)}, 열: {len(df.columns)}")
                
            except Exception as e:
                print(f"  오류: {csv_file} 변환 실패 - {e}")
                import traceback
                traceback.print_exc()
    
    if not success_sheets:
        print("\n[오류] 변환된 시트가 없습니다.")
        sys.exit(1)
    
    print("\n" + "=" * 70)
    print("변환 완료!")
    print(f"생성된 파일: {output_file}")
    print(f"위치: {output_dir}")
    print("=" * 70)
    print()
    print("Excel 파일 구조:")
    for sheet_name in success_sheets:
        print(f"  - 시트: {sheet_name}")
    print()
    
    # 데이터 관계도 출력
    print("데이터 관계도:")
    print("  Character_Stats (스탯 정의)")
    print("  Characters (캐릭터 프로필)")
    print("    - NPC_Favor (호감도 연동)")
    print("  Items_Master")
    print("    - Equipment (장비 아이템)")
    print("    - Gift/Special (선물/특별)")
    print("  Actions (활동/수업/아르바이트/휴식)")
    print("    - Locations (장소 연동)")
    print("  Events")
    print("    - Quests (퀘스트 연동)")
    print("    - NPC_Favor (호감도 연동)")
    print("    - Ending_Conditions (엔딩 조건)")
    print("  Locations (장소 정보)")
    print("    - Actions (활동 장소)")
    print("    - Events (이벤트 장소)")
    print("  Quests (퀘스트 데이터)")
    print("    - Events (퀘스트 이벤트)")
    print("    - Characters (NPC 연동)")
    print("  NPC_Dialogues_Events (대사 이벤트 기본 정보)")
    print("    - NPC_Dialogues_Text (다국어 대사 텍스트)")
    print("      - Language: KO, EN, JP...")
    print("      - Text_Type: Dialogue, Choice, Result")
    print("  CG_Master (CG 이미지)")
    print("  BGM_Master (배경음악)")
    print()

except Exception as e:
    print(f"\n오류 발생: {e}")
    print("pandas와 openpyxl이 설치되어 있는지 확인하세요.")
    print("설치 명령: pip install pandas openpyxl")
    sys.exit(1)
