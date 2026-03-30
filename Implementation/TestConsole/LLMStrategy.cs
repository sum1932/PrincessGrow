using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DessertKingdom.Core.Domain;
using DessertKingdom.Simulation;

namespace DessertKingdom.TestConsole
{
    public class LLMStrategy : ISimulationStrategy
    {
        private readonly ILLMClient _llmClient;
        private readonly List<Activity> _activities;
        private readonly bool _verbose;

        public LLMStrategy(ILLMClient llmClient, List<Activity> activities, bool verbose = false)
        {
            _llmClient = llmClient;
            _activities = activities;
            _verbose = verbose;
        }

        public (Activity Activity1, Activity Activity2) SelectActivities(GameState state)
        {
            var available = _activities.Where(a => a.IsAvailable(state.Turn.CurrentAge, state.Character)).ToList();
            
            if (available.Count == 0)
                return (null, null);

            var prompt = BuildActivityPrompt(state, available);
            
            if (_verbose)
            {
                Console.WriteLine("\n[LLM 프롬프트 - 활동 선택]");
                Console.WriteLine(prompt);
                Console.WriteLine();
            }

            try
            {
                var response = _llmClient.GetCompletionAsync(prompt, 500).GetAwaiter().GetResult();
                
                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("LLM 응답 없음, 랜덤 선택으로 대체");
                    return SelectRandomActivities(available);
                }

                if (_verbose)
                {
                    Console.WriteLine("[LLM 응답]");
                    Console.WriteLine(response);
                    Console.WriteLine();
                }

                // JSON 응답 파싱
                var (index1, index2) = ParseActivityIndices(response, available.Count - 1);
                
                if (index1.HasValue && index1.Value >= 0 && index1.Value < available.Count)
                {
                    var a1 = available[index1.Value];
                    Activity a2 = null;
                    
                    if (index2.HasValue && index2.Value >= 0 && index2.Value < available.Count)
                    {
                        a2 = available[index2.Value];
                    }
                    else
                    {
                        // 두 번째 활동도 랜덤 또는 동일 활동
                        a2 = available[new Random().Next(available.Count)];
                    }

                    if (_verbose)
                    {
                        Console.WriteLine($"[선택된 활동] {a1.Name}, {a2?.Name ?? "없음"}");
                    }

                    return (a1, a2);
                }
                else
                {
                    Console.WriteLine("LLM 응답 파싱 실패, 랜덤 선택으로 대체");
                    return SelectRandomActivities(available);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LLM 호출 오류: {ex.Message}, 랜덤 선택으로 대체");
                return SelectRandomActivities(available);
            }
        }

        public EventChoice MakeChoice(GameEvent gameEvent, List<EventChoice> choices)
        {
            var available = choices.Where(c => c.IsAvailable(null)).ToList();
            if (available.Count == 0)
                return choices.FirstOrDefault();

            var prompt = BuildEventPrompt(gameEvent, available);
            
            if (_verbose)
            {
                Console.WriteLine("\n[LLM 프롬프트 - 이벤트 선택]");
                Console.WriteLine(prompt);
                Console.WriteLine();
            }

            try
            {
                var response = _llmClient.GetCompletionAsync(prompt, 300).GetAwaiter().GetResult();
                
                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("LLM 응답 없음, 랜덤 선택으로 대체");
                    return SelectRandomChoice(available);
                }

                if (_verbose)
                {
                    Console.WriteLine("[LLM 응답]");
                    Console.WriteLine(response);
                    Console.WriteLine();
                }

                var index = ParseChoiceIndex(response, available.Count);
                
                if (index.HasValue && index.Value >= 0 && index.Value < available.Count)
                {
                    var choice = available[index.Value];
                    if (_verbose)
                    {
                        Console.WriteLine($"[선택된 선택지] {choice.Text}");
                    }
                    return choice;
                }
                else
                {
                    Console.WriteLine("LLM 응답 파싱 실패, 랜덤 선택으로 대체");
                    return SelectRandomChoice(available);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LLM 호출 오류: {ex.Message}, 랜덤 선택으로 대체");
                return SelectRandomChoice(available);
            }
        }

        private string BuildActivityPrompt(GameState state, List<Activity> available)
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("당신은 디저트 킹덤 게임의 플레이어입니다. 현재 게임 상태를 분석하고 다음 두 가지 활동을 선택해야 합니다.");
            sb.AppendLine();
            sb.AppendLine("=== 현재 게임 상태 ===");
            sb.AppendLine($"나이: {state.Turn.CurrentAge}세");
            sb.AppendLine($"월: {state.Turn.CurrentMonth}월");
            sb.AppendLine($"턴: {state.Turn.CurrentTurn}");
            sb.AppendLine();
            sb.AppendLine("=== 현재 스탯 ===");
            sb.AppendLine($"체력(HP): {state.Character.GetStat(StatType.HP)}");
            sb.AppendLine($"매력(Charm): {state.Character.GetStat(StatType.Charm)}");
            sb.AppendLine($"지능(Intelligence): {state.Character.GetStat(StatType.Intelligence)}");
            sb.AppendLine($"예술(Art): {state.Character.GetStat(StatType.Art)}");
            sb.AppendLine($"도덕성(Morality): {state.Character.GetStat(StatType.Morality)}");
            sb.AppendLine($"스트레스(Stress): {state.Character.GetStat(StatType.Stress)}");
            sb.AppendLine();
            sb.AppendLine($"자산(스위트): {state.Economy.CurrentMoney}");
            sb.AppendLine();
            sb.AppendLine("=== 선택 가능한 활동 ===");
            
            for (int i = 0; i < available.Count; i++)
            {
                var a = available[i];
                sb.AppendLine($"[{i}] {a.Name} - 비용: {a.Cost}, 수입: {a.Income}");
                if (a.StatEffects.Any())
                {
                    var effects = string.Join(", ", a.StatEffects.Select(e => $"{e.Key}: {(e.Value > 0 ? "+" : "")}{e.Value}"));
                    sb.AppendLine($"    효과: {effects}");
                }
                if (!string.IsNullOrEmpty(a.Description))
                {
                    sb.AppendLine($"    설명: {a.Description}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("=== 선택 지침 ===");
            sb.AppendLine("- 스트레스가 높으면 휴식 활동을 고려하세요.");
            sb.AppendLine("- 목표하는 엔딩에 따라 필요한 스탯을 키우세요.");
            sb.AppendLine("- 재정 상황을 고려하여 활동을 선택하세요.");
            sb.AppendLine();
            sb.AppendLine("JSON 형식으로 응답하세요:");
            sb.AppendLine("{");
            sb.AppendLine('"' + "activity1_index" + '"' + ": 0,  // 첫 번째 활동의 인덱스 번호");
            sb.AppendLine('"' + "activity2_index" + '"' + ": 1,  // 두 번째 활동의 인덱스 번호");
            sb.AppendLine('"' + "reason" + '"' + ": \"선택 이유\"");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string BuildEventPrompt(GameEvent gameEvent, List<EventChoice> choices)
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("이벤트가 발생했습니다! 다음 선택지 중 하나를 선택하세요.");
            sb.AppendLine();
            sb.AppendLine($"=== 이벤트: {gameEvent.Name} ===");
            if (!string.IsNullOrEmpty(gameEvent.Description))
            {
                sb.AppendLine(gameEvent.Description);
            }
            sb.AppendLine();
            sb.AppendLine("=== 선택지 ===");
            
            for (int i = 0; i < choices.Count; i++)
            {
                var c = choices[i];
                sb.AppendLine($"[{i}] {c.Text}");
            }

            sb.AppendLine();
            sb.AppendLine("JSON 형식으로 응답하세요:");
            sb.AppendLine("{");
            sb.AppendLine('"' + "choice_index" + '"' + ": 0,  // 선택할 선택지의 인덱스 번호");
            sb.AppendLine('"' + "reason" + '"' + ": \"선택 이유\"");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private (int? index1, int? index2) ParseActivityIndices(string response, int maxIndex)
        {
            try
            {
                // JSON 파싱 시도
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                var root = doc.RootElement;

                int? idx1 = null, idx2 = null;

                if (root.TryGetProperty("activity1_index", out var prop1))
                {
                    idx1 = prop1.GetInt32();
                }
                else if (root.TryGetProperty("activity1", out var prop1_alt))
                {
                    idx1 = prop1_alt.GetInt32();
                }

                if (root.TryGetProperty("activity2_index", out var prop2))
                {
                    idx2 = prop2.GetInt32();
                }
                else if (root.TryGetProperty("activity2", out var prop2_alt))
                {
                    idx2 = prop2_alt.GetInt32();
                }

                return (idx1, idx2);
            }
            catch
            {
                // JSON 파싱 실패 시 텍스트에서 숫자 추출
                var numbers = new List<int>();
                var words = response.Split(new[] { ' ', '\n', '\r', ',', '.', '"', '\'', '[', ']', '{', '}', ':' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    if (int.TryParse(word, out var num) && num >= 0 && num <= maxIndex)
                    {
                        numbers.Add(num);
                    }
                }

                if (numbers.Count >= 2)
                {
                    return (numbers[0], numbers[1]);
                }
                else if (numbers.Count == 1)
                {
                    return (numbers[0], null);
                }
            }

            return (null, null);
        }

        private int? ParseChoiceIndex(string response, int maxIndex)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.TryGetProperty("choice_index", out var prop))
                {
                    return prop.GetInt32();
                }
                else if (root.TryGetProperty("choice", out var prop2))
                {
                    return prop2.GetInt32();
                }
            }
            catch
            {
                var words = response.Split(new[] { ' ', '\n', '\r', ',', '.', '"', '\'', '[', ']', '{', '}', ':' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    if (int.TryParse(word, out var num) && num >= 0 && num <= maxIndex)
                    {
                        return num;
                    }
                }
            }

            return null;
        }

        private (Activity, Activity) SelectRandomActivities(List<Activity> available)
        {
            var rand = new Random();
            var a1 = available[rand.Next(available.Count)];
            var a2 = available[rand.Next(available.Count)];
            return (a1, a2);
        }

        private EventChoice SelectRandomChoice(List<EventChoice> choices)
        {
            return choices[new Random().Next(choices.Count)];
        }
    }
}
