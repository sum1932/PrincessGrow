using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DessertKingdom.TestConsole
{
    public class KimiClient : ILLMClient
    {
        public Task<string> GetCompletionAsync(string prompt, int maxTokens = 500)
        {
            string response = GenerateResponse(prompt);
            return Task.FromResult(response);
        }

        private string GenerateResponse(string prompt)
        {
            try
            {
                if (prompt.Contains("활동을 선택") || prompt.Contains("activities"))
                {
                    return SelectActivityJson(prompt);
                }
                else if (prompt.Contains("이벤트") || prompt.Contains("event"))
                {
                    return SelectEventChoiceJson(prompt);
                }
                else
                {
                    return "{\"activity1\": \"\", \"activity2\": \"\", \"reason\": \"프롬프트를 이해할 수 없습니다\"}";
                }
            }
            catch (Exception ex)
            {
                return $"{{\"activity1\": \"\", \"activity2\": \"\", \"reason\": \"오류: {ex.Message}\"}}";
            }
        }

        private string SelectActivityJson(string prompt)
        {
            int hp = ExtractStat(prompt, "체력");
            int charm = ExtractStat(prompt, "매력");
            int intelligence = ExtractStat(prompt, "지능");
            int art = ExtractStat(prompt, "예술");
            int morality = ExtractStat(prompt, "도덕");
            int stress = ExtractStat(prompt, "스트레스");
            int money = ExtractStat(prompt, "자금");
            int age = ExtractAge(prompt);

            var activities = ExtractActivities(prompt);
            var (index1, index2, reason) = StrategyBasedSelection(hp, charm, intelligence, art, morality, stress, money, age, activities);
            
            var result = new
            {
                activity1_index = index1,
                activity2_index = index2,
                reason = reason
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false 
            });
        }

        private string SelectEventChoiceJson(string prompt)
        {
            var result = new
            {
                choice = 0,
                reason = "이벤트 선택: 첫 번째 옵션"
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false 
            });
        }

        private (int index1, int index2, string reason) StrategyBasedSelection(
            int hp, int charm, int intelligence, int art, int morality, int stress, int money, int age, 
            List<string> activities)
        {
            int index1 = 0;
            int index2 = -1;
            string reason = "";

            if (stress >= 80)
            {
                var rest = activities.Select((a, i) => new { Activity = a, Index = i })
                                     .FirstOrDefault(x => x.Activity.Contains("쉬기") || x.Activity.Contains("산책") || x.Activity.Contains("여행"));
                if (rest != null)
                {
                    index1 = rest.Index;
                    reason = $"스트레스가 {stress}로 높아 휴식이 필요합니다.";
                    return (index1, index2, reason);
                }
            }

            if (money < 200)
            {
                var job = activities.Select((a, i) => new { Activity = a, Index = i })
                                    .FirstOrDefault(x => x.Activity.Contains("알바") && !x.Activity.Contains("지능 -15"));
                if (job != null)
                {
                    index1 = job.Index;
                    reason = $"자금이 {money}로 부족해 수입 활동이 필요합니다.";
                    return (index1, index2, reason);
                }
            }

            if (age < 12)
            {
                if (intelligence < 100)
                {
                    var study = activities.Select((a, i) => new { Activity = a, Index = i })
                                          .FirstOrDefault(x => x.Activity.Contains("학업 공부"));
                    if (study != null)
                    {
                        index1 = study.Index;
                        reason = $"{age}세: 지능({intelligence}) 향상이 시급합니다.";
                        return (index1, index2, reason);
                    }
                }
                if (hp < 80)
                {
                    var physical = activities.Select((a, i) => new { Activity = a, Index = i })
                                             .FirstOrDefault(x => x.Activity.Contains("체력 단련"));
                    if (physical != null)
                    {
                        index1 = physical.Index;
                        reason = $"{age}세: 체력({hp}) 강화가 필요합니다.";
                        return (index1, index2, reason);
                    }
                }
            }

            var stats = new[] { ("체력", hp), ("매력", charm), ("지능", intelligence), ("예술", art), ("도덕", morality) };
            var lowest = stats.OrderBy(s => s.Item2).First();

            string targetActivity = lowest.Item1 switch
            {
                "체력" => "체력 단련",
                "매력" => "매력 수업",
                "지능" => "학업 공부",
                "예술" => "예술 수업",
                "도덕" => "도덕 수업",
                _ => "종합 교육"
            };

            var matched = activities.Select((a, i) => new { Activity = a, Index = i })
                                    .FirstOrDefault(x => x.Activity.Contains(targetActivity));
            if (matched != null)
            {
                index1 = matched.Index;
                reason = $"{lowest.Item1}({lowest.Item2})가 가장 낮아 보완이 필요합니다.";
                return (index1, index2, reason);
            }

            var all = activities.Select((a, i) => new { Activity = a, Index = i })
                                .FirstOrDefault(x => x.Activity.Contains("종합 교육"));
            if (all != null)
            {
                index1 = all.Index;
                reason = "균형 잡힌 성장을 위해 종합 교육을 선택했습니다.";
            }
            else
            {
                index1 = 0;
                reason = "기본 선택입니다.";
            }

            return (index1, index2, reason);
        }

        private string ExtractActivityName(string activityLine)
        {
            try
            {
                var parts = activityLine.Split('|');
                if (parts.Length > 0)
                {
                    var namePart = parts[0].Trim();
                    var dotIndex = namePart.IndexOf('.');
                    if (dotIndex > 0)
                    {
                        return namePart.Substring(dotIndex + 1).Trim();
                    }
                }
            }
            catch { }
            return activityLine.Trim();
        }

        private int ExtractStat(string text, string statName)
        {
            try
            {
                var pattern = $"{statName}\\s*[:：]\\s*([0-9]+)";
                var match = Regex.Match(text, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    return value;
            }
            catch { }
            return 50;
        }

        private int ExtractAge(string text)
        {
            try
            {
                var pattern = "나이\\s*[:：]\\s*([0-9]+)";
                var match = Regex.Match(text, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    return value;
            }
            catch { }
            return 5;
        }

        private List<string> ExtractActivities(string text)
        {
            var activities = new List<string>();
            try
            {
                var lines = text.Split('\n');
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, "^\\s*[0-9]+\\."))
                    {
                        activities.Add(line.Trim());
                    }
                }
            }
            catch { }
            return activities;
        }
    }
}
