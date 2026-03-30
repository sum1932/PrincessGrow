@echo off
chcp 65001 >nul
echo.
echo 🎮 디저트 킹덤 웹 테스트 서버
echo =================================
echo.

REM WebSimulation 폴더로 이동
cd /d "%~dp0WebSimulation"

REM Node.js 설치 확인
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Node.js가 설치되어 있지 않습니다!
    echo    https://nodejs.org 에서 설치해주세요.
    pause
    exit /b 1
)

echo ✅ Node.js 확인 완료
echo.

REM 의존성 설치 확인
if not exist "node_modules" (
    echo 📦 의존성 패키지 설치 중...
    npm install
    if errorlevel 1 (
        echo ❌ 패키지 설치 실패!
        pause
        exit /b 1
    )
    echo ✅ 패키지 설치 완료
    echo.
)

echo 🚀 서버를 시작합니다...
echo    주소: http://localhost:3001
echo    종료하려면 Ctrl+C를 누르세요.
echo.
echo =================================

npm start

pause
