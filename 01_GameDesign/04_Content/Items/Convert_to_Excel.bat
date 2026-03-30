@echo off
chcp 65001 >nul
title CSV to Excel 변환

REM 현재 디렉토리 저장
set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

echo ============================================
echo    CSV to Excel 변환기
echo ============================================
echo.
echo 작업 디렉토리: %CD%
echo.

REM Python 확인
python --version >nul 2>&1
if errorlevel 1 (
    echo Python이 설치되지 않았습니다.
    echo Python을 먼저 설치해주세요: https://www.python.org/downloads/
    pause
    exit /b 1
)

echo Python 확인 완료

REM 필요한 패키지 설치
echo.
echo 필요한 패키지 설치 중...
python -m pip install pandas openpyxl -q
if errorlevel 1 (
    echo 패키지 설치 중 문제가 발생했습니다.
    echo 수동으로 설치하려면 다음 명령을 실행하세요:
    echo pip install pandas openpyxl
)

echo 패키지 준비 완료
echo.

REM CSV 파일 존재 확인
echo CSV 파일 확인 중...
if not exist "Items_Master.csv" (
    echo 오류: Items_Master.csv 파일을 찾을 수 없습니다.
    pause
    exit /b 1
)

echo CSV 파일 확인 완료
echo.

REM 스크립트 실행
echo 변환 시작...
python "convert_csv_to_excel.py"
if errorlevel 1 (
    echo.
    echo 변환 중 오류가 발생했습니다.
    pause
    exit /b 1
)

echo.
echo ============================================
echo    변환 완료!
echo ============================================
echo.
echo Items_Data.xlsx 파일이 생성되었습니다.
echo 위치: %CD%\Items_Data.xlsx
echo.
pause
