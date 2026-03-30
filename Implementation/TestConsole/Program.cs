using System;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Data;
using DessertKingdom.Core.Domain;
using DessertKingdom.Core.Services;
using DessertKingdom.Simulation;

namespace DessertKingdom.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== 디저트 킹덤 시뮬레이션 테스트 ===\n");

            // 1. Repository 초기화
            var activityRepo = new CsvActivityRepository();
            var eventRepo = new JsonEventRepository();
            var endingRepo = new JsonEndingRepository();

            // 데이터 로드 확인
            Console.WriteLine($"활동 수: {activityRepo.GetAll().Count}");
            Console.WriteLine($"이벤트 수: {eventRepo.GetAll().Count}");
            Console.WriteLine($"엔딩 수: {endingRepo.GetAll().Count}");
            Console.WriteLine();

            // 전략 선택
            Console.WriteLine("시뮬레이션 모드를 선택하세요:");
            Console.WriteLine("1. 인터랙티브 모드 (직접 선택)");
            Console.WriteLine("2. 랜덤 전략 (Random)");
            Console.WriteLine("3. LLM 전략 (AI 기반 선택)");
            Console.Write("선택 (1/2/3): ");
            
            var choice = Console.ReadLine()?.Trim() ?? "1";
            
            if (choice == "1")
            {
                // 인터랙티브 모드
                var interactive = new InteractiveSimulation();
                interactive.Start();
                return;
            }
            
            ISimulationStrategy strategy;
            bool useLLM = false;
            bool verbose = false;
            
            if (choice == "3")
            {
                // LLM 제공자 선택
                Console.WriteLine("\nLLM API 제공자를 선택하세요:");
                Console.WriteLine("1. OpenAI (GPT-3.5/GPT-4)");
                Console.WriteLine("2. Google Gemini (Gemini 2.0 Flash)");
                Console.WriteLine("3. Kimi 2.5 (Moonshot AI)");
                Console.Write("선택 (1/2/3): ");
                var apiChoice = Console.ReadLine()?.Trim() ?? "1";
                
                ILLMClient llmClient = null;
                string apiKey = null;
                bool initialized = false;
                
                if (apiChoice == "2")
                {
                    // Gemini 선택
                    apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        Console.WriteLine("경고: GEMINI_API_KEY 환경변수가 설정되지 않았습니다.");
                        Console.WriteLine("API 키를 직접 입력하세요 (Enter 키로 건너뛰면 랜덤 전략 사용):");
                        apiKey = Console.ReadLine()?.Trim();
                        
                        if (!string.IsNullOrEmpty(apiKey))
                        {
                            try
                            {
                                llmClient = new GeminiClient(apiKey);
                                useLLM = true;
                                initialized = true;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Gemini 클라이언트 초기화 오류: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        llmClient = new GeminiClient(apiKey);
                        useLLM = true;
                        initialized = true;
                    }
                }
                else if (apiChoice == "3")
                {
                    // Kimi 선택 - API 키 없이 로컬 AI 사용
                    Console.WriteLine("\nKimi 2.5 (로컬 AI)를 사용합니다. API 키가 필요하지 않습니다.");
                    try
                    {
                        llmClient = new KimiClient();
                        useLLM = true;
                        initialized = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Kimi 클라이언트 초기화 오류: {ex.Message}");
                    }
                }
                else
                {
                    // OpenAI 선택
                    apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        Console.WriteLine("경고: OPENAI_API_KEY 환경변수가 설정되지 않았습니다.");
                        Console.WriteLine("API 키를 직접 입력하세요 (Enter 키로 건너뛰면 랜덤 전략 사용):");
                        apiKey = Console.ReadLine()?.Trim();
                        
                        if (!string.IsNullOrEmpty(apiKey))
                        {
                            try
                            {
                                llmClient = new OpenAIClient(apiKey);
                                useLLM = true;
                                initialized = true;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"OpenAI 클라이언트 초기화 오류: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        llmClient = new OpenAIClient(apiKey);
                        useLLM = true;
                        initialized = true;
                    }
                }

                if (useLLM && llmClient != null && initialized)
                {
                    Console.Write("상세 출력 모드? (y/n): ");
                    verbose = Console.ReadLine()?.Trim().ToLower() == "y";
                    
                    strategy = new LLMStrategy(llmClient, activityRepo.GetAll(), verbose);
                    string providerName = apiChoice switch
                    {
                        "2" => "Gemini",
                        "3" => "Kimi 2.5",
                        _ => "OpenAI"
                    };
                    Console.WriteLine($"LLM 전략 ({providerName})으로 실행합니다.\n");
                }
                else
                {
                    Console.WriteLine("LLM 초기화 실패. 랜덤 전략으로 전환합니다.");
                    strategy = new RandomStrategy(activityRepo.GetAll());
                    useLLM = false;
                }
            }
            else
            {
                strategy = new RandomStrategy(activityRepo.GetAll());
            }

            // 2. 단일 시뮬레이션 실행
            Console.WriteLine("--- 단일 시뮬레이션 ---");
            RunSingleSimulation(activityRepo, eventRepo, endingRepo, strategy, useLLM);

            // 3. 대량 시뮬레이션 실행 (LLM은 1회만 권장)
            if (!useLLM)
            {
                Console.WriteLine("\n--- 대량 시뮬레이션 (100회) ---");
                RunMassSimulation(activityRepo, eventRepo, endingRepo, 100);
            }
            else
            {
                Console.WriteLine("\nLLM 전략은 API 비용 문제로 대량 시뮬레이션을 건너뜁니다.");
            }
            
            Console.WriteLine("\n아무 키나 눌러 종료...");
            Console.ReadKey();
        }

        private static void RunSingleSimulation(
            IActivityRepository activityRepo, 
            IEventRepository eventRepo, 
            IEndingRepository endingRepo,
            ISimulationStrategy strategy,
            bool useLLM = false)
        {
            // 게임 상태 초기화
            var state = new GameState();
            
            // 서비스 초기화
            var eventManager = new EventManager(eventRepo.GetAll());
            var endingJudge = new EndingJudge(endingRepo.GetAll());
            var turnManager = new TurnManager(state, eventManager);

            // 시뮬레이터 초기화
            var simulator = new GameSimulator(turnManager, eventManager, endingJudge, state);
            simulator.SetStrategy(strategy);

            // 시뮬레이션 실행
            var result = simulator.RunFullSimulation();

            // 결과 출력
            Console.WriteLine($"총 턴 수: {result.TurnResults.Count}");
            Console.WriteLine($"최종 나이: {result.FinalState.Turn.CurrentAge}세");
            Console.WriteLine($"최종 월: {result.FinalState.Turn.CurrentMonth}월");
            Console.WriteLine();
            
            Console.WriteLine("[최종 스탯]");
            Console.WriteLine($"  체력(HP): {result.FinalState.Character.GetStat(StatType.HP)}");
            Console.WriteLine($"  매력(Charm): {result.FinalState.Character.GetStat(StatType.Charm)}");
            Console.WriteLine($"  지능(Intelligence): {result.FinalState.Character.GetStat(StatType.Intelligence)}");
            Console.WriteLine($"  예술(Art): {result.FinalState.Character.GetStat(StatType.Art)}");
            Console.WriteLine($"  도덕성(Morality): {result.FinalState.Character.GetStat(StatType.Morality)}");
            Console.WriteLine($"  스트레스(Stress): {result.FinalState.Character.GetStat(StatType.Stress)}");
            Console.WriteLine();

            Console.WriteLine("[최종 자산]");
            Console.WriteLine($"  자산: {result.FinalState.Economy.CurrentMoney}");
            Console.WriteLine();

            Console.WriteLine($"[엔딩] {result.Ending?.Name ?? "없음"}");
            if (result.Ending != null)
            {
                Console.WriteLine($"  우선순위: {result.Ending.Priority}");
                Console.WriteLine($"  설명: {result.Ending.Description}");
            }

            // 발생한 이벤트 요약
            var triggeredEvents = result.TurnResults
                .SelectMany(r => r.TriggeredEvents)
                .GroupBy(e => e.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            if (triggeredEvents.Any())
            {
                Console.WriteLine("\n[발생한 이벤트]");
                foreach (var evt in triggeredEvents)
                {
                    Console.WriteLine($"  {evt.Name}: {evt.Count}회");
                }
            }
        }

        private static void RunMassSimulation(
            IActivityRepository activityRepo, 
            IEventRepository eventRepo, 
            IEndingRepository endingRepo,
            int count)
        {
            var endingCounts = new Dictionary<string, int>();
            var totalTurns = 0;

            for (int i = 0; i < count; i++)
            {
                var state = new GameState();
                var eventManager = new EventManager(eventRepo.GetAll());
                var endingJudge = new EndingJudge(endingRepo.GetAll());
                var turnManager = new TurnManager(state, eventManager);
                
                var simulator = new GameSimulator(turnManager, eventManager, endingJudge, state);
                var strategy = new RandomStrategy(activityRepo.GetAll());
                simulator.SetStrategy(strategy);

                var result = simulator.RunFullSimulation();

                totalTurns += result.TurnResults.Count;

                var endingName = result.Ending?.Name ?? "없음";
                if (!endingCounts.ContainsKey(endingName))
                    endingCounts[endingName] = 0;
                endingCounts[endingName]++;
            }

            Console.WriteLine($"평균 턴 수: {(double)totalTurns / count:F1}");
            Console.WriteLine("\n[엔딩 분포]");
            
            foreach (var ending in endingCounts.OrderByDescending(x => x.Value))
            {
                var percentage = (ending.Value * 100.0 / count);
                Console.WriteLine($"  {ending.Key}: {ending.Value}회 ({percentage:F1}%)");
            }
        }
    }
}
