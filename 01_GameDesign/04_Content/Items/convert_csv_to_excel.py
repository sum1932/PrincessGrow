#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
CSV to Excel 변환 스크립트
Items 폴더의 CSV 파일들을 하나의 Excel 파일로 통합
"""

import pandas as pd
import os
import sys
from pathlib import Path

print("=" * 60)
print("CSV to Excel 변환 시작")
print("=" * 60)

# 현재 작업 디렉토리 (스크립트가 있는 폴더)
current_dir = Path.cwd()
print(f"\n작업 디렉토리: {current_dir}")

# 변환할 CSV 파일 목록
csv_files = {
    'Items_Master': 'Items_Master.csv',
    'Equipment': 'Equipment.csv',
    'Gifts': 'Gifts.csv',
    'SpecialItems': 'SpecialItems.csv'
}

# 출력 Excel 파일명
output_file = 'Items_Data.xlsx'

print(f"\n출력 파일: {output_file}")
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
    
    # 주석 제거 (비어있는 줄도 제거)
    clean_lines = []
    for line in lines:
        stripped = line.strip()
        # 비어있는 줄이나 주석(#으로 시작)이 아닌 줄만 추가
        if stripped and not stripped.startswith('#'):
            clean_lines.append(line)
    
    # 임시 파일로 저장 후 pandas로 읽기
    import tempfile
    with tempfile.NamedTemporaryFile(mode='w', encoding='utf-8', suffix='.csv', delete=False) as tmp:
        tmp.writelines(clean_lines)
        tmp_path = tmp.name
    
    try:
        df = pd.read_csv(tmp_path, encoding='utf-8')
    finally:
        os.unlink(tmp_path)  # 임시 파일 삭제
    
    return df

# CSV 파일 존재 확인
missing_files = []
for csv_file in csv_files.values():
    if not (current_dir / csv_file).exists():
        missing_files.append(csv_file)

if missing_files:
    print(f"오류: 다음 파일을 찾을 수 없습니다:")
    for f in missing_files:
        print(f"  - {f}")
    print("\n파일을 확인하고 다시 실행해주세요.")
    sys.exit(1)

# Excel 파일 생성
try:
    with pd.ExcelWriter(output_file, engine='openpyxl') as writer:
        for sheet_name, csv_file in csv_files.items():
            try:
                print(f"\n처리 중: {csv_file}...")
                
                # 주석 처리 기능으로 CSV 읽기
                df = read_csv_with_comments(csv_file)
                
                # Excel 시트로 저장
                df.to_excel(writer, sheet_name=sheet_name, index=False)
                
                print(f"  완료: {sheet_name} (행: {len(df)}, 열: {len(df.columns)})")
                
            except Exception as e:
                print(f"  오류: {csv_file} 변환 실패 - {e}")
                import traceback
                traceback.print_exc()
    
    print("=" * 60)
    print("변환 완료!")
    print(f"생성된 파일: {output_file}")
    print(f"위치: {current_dir / output_file}")
    print("=" * 60)
    print()
    print("Excel 파일 구조:")
    for sheet_name in csv_files.keys():
        print(f"  - 시트: {sheet_name}")
    print()

except Exception as e:
    print(f"\n오류 발생: {e}")
    print("pandas와 openpyxl이 설치되어 있는지 확인하세요.")
    print("설치 명령: pip install pandas openpyxl")
    sys.exit(1)
