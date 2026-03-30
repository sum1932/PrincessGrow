using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DessertKingdom.Core.Data;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.TestConsole
{
    public class InteractiveSimulation
    {
        private readonly IActivityRepository _activityRepo;
        private readonly IEventRepository _eventRepo;
        private readonly IEndingRepository _endingRepo;
        private GameState _gameState;
        private int _totalTurns = 180;
        private int _currentTurn = 0;
        private List<string> _simulationLog;
        private string _logFolderPath;

        public InteractiveSimulation()
        {
            _activityRepo = new CsvActivityRepository();
            _eventRepo = new JsonEventRepository();
            _endingRepo = new JsonEndingRepository();
            _simulationLog = new List<string>();
            
            // 로그 폴더 설정
            _logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimulationLogs");
            if (!Directory.Exists(_logFolderPath))
                Directory.CreateDirectory(_logFolderPath);
            
            Console.WriteLine($"데이터 로드 완료: 활동 {_activityRepo.GetAll().Count}개, 이벤트 {_eventRepo.GetAll().Count}개, 엔딩 {_endingRepo.GetAll().Count}개");
            Console.WriteLine($"로그 저장 위치: {_logFolderPath}\n");
        }

        public void Start()
        {
            Console.WriteLine("=== 디저트 킹덤 인터랙티브 시뮬레이션 ===\n");
            
            ConfigureSimulation();
            InitializeGameState();
            RunGameLoop();
        }

        private void ConfigureSimulation()
        {
            Console.WriteLine("[시뮬레이션 설정]\n");
            
            Console.WriteLine("게임 기간을 선택하세요:");
            Console.WriteLine("1. 10년 (120턴) - 빠른 테스트");
            Console.WriteLine("2. 15년 (180턴) - 표준");
            Console.WriteLine("3. 20년 (240턴) - 장기");
            Console.Write("선택: ");
            
            var choice = Console.ReadLine()?.Trim();
            switch (choice)
            {
                case "1": _totalTurns = 120; break;
                case "2": _totalTurns = 180; break;
                case "3": _totalTurns = 240; break;
                default: _totalTurns = 180; break;
            }
            
            Log($"시뮬레이션 설정: {_totalTurns}턴");
            Console.WriteLine($"총 {_totalTurns}턴으로 설정되었습니다.\n");
        }

        private void InitializeGameState()
        {
            Console.WriteLine("[캐릭터 초기화]\n");
            
            _gameState = new GameState();
            
            // 게임 기획에 따른 초기값:
            // 모든 스탯 50, 스트레스 0, 자금 500
            // CharacterStats 생성자에서 이미 HP=50, Charm=50, Intelligence=50, Art=50, Morality=50, Stress=0
            // Economy 생성자에서 이미 CurrentMoney=500
            
            Console.WriteLine("캐릭터 '루아'가 생성되었습니다.");
            Console.WriteLine("초기 스탯:");
            Console.WriteLine($"  체력: {_gameState.Character.GetStat(StatType.HP)} | 매력: {_gameState.Character.GetStat(StatType.Charm)} | 지능: {_gameState.Character.GetStat(StatType.Intelligence)}");
            Console.WriteLine($"  예술: {_gameState.Character.GetStat(StatType.Art)} | 도덕: {_gameState.Character.GetStat(StatType.Morality)} | 스트레스: {_gameState.Character.GetStat(StatType.Stress)}");
            Console.WriteLine($"  자금: {_gameState.Economy.CurrentMoney} Sweets\n");
            
            Log($"초기 스탯 - 체력:{_gameState.Character.GetStat(StatType.HP)} 매력:{_gameState.Character.GetStat(StatType.Charm)} 지능:{_gameState.Character.GetStat(StatType.Intelligence)} 예술:{_gameState.Character.GetStat(StatType.Art)} 도덕:{_gameState.Character.GetStat(StatType.Morality)} 스트레스:{_gameState.Character.GetStat(StatType.Stress)} 자금:{_gameState.Economy.CurrentMoney}");
        }

        private void RunGameLoop()
        {
            while (_currentTurn < _totalTurns)
            {
                int year = 5 + (_currentTurn / 12);
                int month = (_currentTurn % 12) + 1;
                int age = 5 + (_currentTurn / 12);
                
                var turnHeader = $"[턴 {_currentTurn + 1}/{_totalTurns}] {year}년 {month}월 - 나이: {age}세";
                Console.WriteLine($"\n{'='.ToString().PadRight(60, '=')}");
                Console.WriteLine(turnHeader);
                Console.WriteLine($"{'='.ToString().PadRight(60, '=')}\n");
                
                Log($"\n{turnHeader}");
                DisplayStatus();
                
                // 가능한 활동 표시
                var availableActivities = _activityRepo.GetAvailable(age, _gameState.Character);
                
                if (availableActivities.Count == 0)
                {
                    Console.WriteLine("[경고] 선택 가능한 활동이 없습니다!");
                    Log("경고: 선택 가능한 활동 없음");
                    break;
                }
                
                DisplayActivities(availableActivities);
                
                Console.Write("\n선택 (번호/q): ");
                var input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(input))
                    continue;
                
                if (input.ToLower() == "q" || input.ToLower() == "quit")
                {
                    Console.WriteLine("\n시뮬레이션을 종료합니다.");
                    Log("사용자 종료");
                    SaveLog();
                    break;
                }
                
                if (int.TryParse(input, out int activityIndex) && activityIndex > 0 && activityIndex <= availableActivities.Count)
                {
                    var selectedActivity = availableActivities[activityIndex - 1];
                    ExecuteActivity(selectedActivity);
                    
                    _currentTurn++;
                    
                    // 엔딩 체크
                    var ending = CheckEnding();
                    if (ending != null)
                    {
                        DisplayEnding(ending);
                        SaveLog();
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다. 다시 선택해주세요.");
                }
            }
            
            if (_currentTurn >= _totalTurns)
            {
                Console.WriteLine("\n[시뮬레이션 종료]");
                Console.WriteLine($"총 {_totalTurns}턴이 경과했습니다.");
                Log($"\n시뮬레이션 종료 - 총 {_totalTurns}턴 경과");
                DisplayFinalStats();
                SaveLog();
            }
        }

        private void DisplayStatus()
        {
            var status = $"자금:{_gameState.Economy.CurrentMoney} Sweets | 스트레스:{_gameState.Character.GetStat(StatType.Stress)}/100 | 체력:{_gameState.Character.GetStat(StatType.HP)} | 매력:{_gameState.Character.GetStat(StatType.Charm)} | 지능:{_gameState.Character.GetStat(StatType.Intelligence)} | 예술:{_gameState.Character.GetStat(StatType.Art)} | 도덕:{_gameState.Character.GetStat(StatType.Morality)}";
            
            Console.WriteLine("[현재 상태]");
            Console.WriteLine($"  {status}\n");
            Log($"상태 - {status}");
        }

        private void DisplayActivities(List<Activity> activities)
        {
            Console.WriteLine("[가능한 활동]");
            Console.WriteLine();
            
            // 활동 유형별로 그룹화
            var grouped = activities.GroupBy(a => a.Type).OrderBy(g => g.Key);
            int index = 1;
            
            foreach (var group in grouped)
            {
                string typeName = GetActivityTypeName(group.Key);
                Console.WriteLine($"=== {typeName} ===");
                
                foreach (var activity in group)
                {
                    var statEffects = activity.StatEffects;
                    string effectStr = "";
                    
                    if (statEffects.Count > 0)
                    {
                        var effects = new List<string>();
                        foreach (var effect in statEffects)
                        {
                            string sign = effect.Value >= 0 ? "+" : "";
                            string statName = GetStatName(effect.Key);
                            effects.Add($"{statName}{sign}{effect.Value}");
                        }
                        effectStr = string.Join(", ", effects);
                    }
                    
                    string costStr = "";
                    if (activity.Cost > 0)
                        costStr = $"비용:{activity.Cost} ";
                    else if (activity.Income > 0)
                        costStr = $"수입:{activity.Income} ";
                    
                    string stressStr = "";
                    if (activity.StressChange > 0)
                        stressStr = $"스트레스:+{activity.StressChange}";
                    else if (activity.StressChange < 0)
                        stressStr = $"스트레스:{activity.StressChange}";
                    
                    Console.WriteLine($"  {index,2}. {activity.Name,-15} | {costStr,-12} | {effectStr,-30} {stressStr}");
                    index++;
                }
                Console.WriteLine();
            }
            
            Console.WriteLine("[명령어] q=종료");
        }

        private string GetActivityTypeName(ActivityType type)
        {
            return type switch
            {
                ActivityType.Lesson => "수업",
                ActivityType.PartTime => "아르바이트",
                ActivityType.Rest => "휴식",
                ActivityType.Outing => "외출",
                ActivityType.Special => "특별",
                _ => "기타"
            };
        }

        private string GetStatName(StatType type)
        {
            return type switch
            {
                StatType.HP => "체력",
                StatType.Charm => "매력",
                StatType.Intelligence => "지능",
                StatType.Art => "예술",
                StatType.Morality => "도덕",
                StatType.Stress => "스트레스",
                _ => type.ToString()
            };
        }

        private void ExecuteActivity(Activity activity)
        {
            Console.WriteLine($"\n[활동 실행] {activity.Name}");
            Console.WriteLine($"설명: {activity.Description}\n");
            
            Log($"활동 선택: {activity.Name}");
            
            // 자금 처리
            if (activity.Cost > 0)
            {
                for (int i = 0; i < activity.Cost; i++)
                    _gameState.Economy.Spend(1, activity.Name);
            }
            if (activity.Income > 0)
                _gameState.Economy.Earn(activity.Income, activity.Name);
            
            // 스탯 변화 적용
            var changes = new List<string>();
            foreach (var effect in activity.StatEffects)
            {
                int oldValue = _gameState.Character.GetStat(effect.Key);
                _gameState.Character.ModifyStat(effect.Key, effect.Value);
                int newValue = _gameState.Character.GetStat(effect.Key);
                
                string sign = effect.Value >= 0 ? "+" : "";
                changes.Add($"{GetStatName(effect.Key)} {sign}{effect.Value} ({oldValue}->{newValue})");
            }
            
            // 스트레스 변화
            if (activity.StressChange != 0)
            {
                int oldStress = _gameState.Character.GetStat(StatType.Stress);
                _gameState.Character.ModifyStat(StatType.Stress, activity.StressChange);
                int newStress = _gameState.Character.GetStat(StatType.Stress);
                
                string sign = activity.StressChange >= 0 ? "+" : "";
                changes.Add($"스트레스 {sign}{activity.StressChange} ({oldStress}->{newStress})");
            }
            
            // 결과 표시
            Console.WriteLine("[변화]");
            if (activity.Cost > 0) 
            {
                Console.WriteLine($"  자금 -{activity.Cost} (남은 자금: {_gameState.Economy.CurrentMoney})");
                Log($"  자금 -{activity.Cost} (남은 자금: {_gameState.Economy.CurrentMoney})");
            }
            if (activity.Income > 0) 
            {
                Console.WriteLine($"  자금 +{activity.Income} (현재 자금: {_gameState.Economy.CurrentMoney})");
                Log($"  자금 +{activity.Income} (현재 자금: {_gameState.Economy.CurrentMoney})");
            }
            
            foreach (var change in changes)
            {
                Console.WriteLine($"  {change}");
                Log($"  {change}");
            }
            
            Console.WriteLine("\n아무 키나 눌러 계속...");
            Console.ReadKey(true);
        }

        private EndingCondition CheckEnding()
        {
            var endings = _endingRepo.GetAll();
            foreach (var ending in endings)
            {
                if (ending.IsMet(_gameState))
                    return ending;
            }
            return null;
        }

        private void DisplayEnding(EndingCondition ending)
        {
            Console.WriteLine("\n" + "=".PadRight(60, '='));
            Console.WriteLine("[엔딩 도달!]");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine($"\n엔딩: {ending.Name}");
            Console.WriteLine($"설명: {ending.Description}\n");
            
            Log($"\n[엔딩 도달] {ending.Name}");
            Log($"설명: {ending.Description}");
            
            DisplayFinalStats();
        }

        private void DisplayFinalStats()
        {
            int finalAge = 5 + (_currentTurn / 12);
            
            Console.WriteLine("[최종 스탯]");
            Console.WriteLine($"  나이: {finalAge}세");
            Console.WriteLine($"  자금: {_gameState.Economy.CurrentMoney} Sweets");
            Console.WriteLine($"  스트레스: {_gameState.Character.GetStat(StatType.Stress)}");
            Console.WriteLine($"  체력: {_gameState.Character.GetStat(StatType.HP)}");
            Console.WriteLine($"  매력: {_gameState.Character.GetStat(StatType.Charm)}");
            Console.WriteLine($"  지능: {_gameState.Character.GetStat(StatType.Intelligence)}");
            Console.WriteLine($"  예술: {_gameState.Character.GetStat(StatType.Art)}");
            Console.WriteLine($"  도덕: {_gameState.Character.GetStat(StatType.Morality)}");
            
            Log($"\n[최종 스탯]");
            Log($"  나이: {finalAge}세");
            Log($"  자금: {_gameState.Economy.CurrentMoney} Sweets");
            Log($"  스트레스: {_gameState.Character.GetStat(StatType.Stress)}");
            Log($"  체력: {_gameState.Character.GetStat(StatType.HP)}");
            Log($"  매력: {_gameState.Character.GetStat(StatType.Charm)}");
            Log($"  지능: {_gameState.Character.GetStat(StatType.Intelligence)}");
            Log($"  예술: {_gameState.Character.GetStat(StatType.Art)}");
            Log($"  도덕: {_gameState.Character.GetStat(StatType.Morality)}");
        }

        private void Log(string message)
        {
            _simulationLog.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }

        private void SaveLog()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"SimulationLog_{timestamp}.md";
                string filePath = Path.Combine(_logFolderPath, fileName);
                
                var sb = new StringBuilder();
                sb.AppendLine("# 디저트 킹덤 시뮬레이션 로그");
                sb.AppendLine();
                sb.AppendLine($"**실행 시간:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"**총 턴:** {_currentTurn}/{_totalTurns}");
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
                
                foreach (var logEntry in _simulationLog)
                {
                    sb.AppendLine(logEntry);
                }
                
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                
                Console.WriteLine($"\n[로그 저장 완료]");
                Console.WriteLine($"파일: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[로그 저장 실패] {ex.Message}");
            }
        }
    }
}
