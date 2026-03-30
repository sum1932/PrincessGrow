using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DessertKingdom.TestConsole
{
    public class OpenAIClient : ILLMClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public OpenAIClient(string apiKey, string model = "gpt-3.5-turbo")
        {
            _apiKey = apiKey ?? throw new ArgumentException("OPENAI_API_KEY 환경변수가 필요합니다.");
            _model = model;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 500)
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "당신은 디저트 킹덤 게임의 플레이어입니다. 게임 상태를 분석하고 최선의 선택을 해야 합니다." },
                    new { role = "user", content = prompt }
                },
                max_tokens = maxTokens,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"OpenAI API 오류: {responseString}");
                    return null;
                }

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;
                var choices = root.GetProperty("choices");
                var message = choices[0].GetProperty("message");
                return message.GetProperty("content").GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI 호출 오류: {ex.Message}");
                return null;
            }
        }
    }
}
