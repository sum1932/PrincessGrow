using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

namespace DessertKingdom.TestConsole
{
    public class GeminiClient : ILLMClient
    {
        private readonly Client _client;
        private readonly string _model;

        public GeminiClient(string apiKey, string model = "gemini-2.0-flash-lite-preview-02-05")
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("GEMINI_API_KEY 환경변수가 필요합니다.");
            
            _client = new Client(apiKey: apiKey);
            _model = model;
        }

        public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 500)
        {
            var contents = new List<Content>
            {
                new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        new Part { Text = "당신은 디저트 킹덤 게임의 플레이어입니다. 게임 상태를 분석하고 최선의 선택을 해야 합니다.\n\n" + prompt }
                    }
                }
            };

            var config = new GenerateContentConfig
            {
                MaxOutputTokens = maxTokens,
                Temperature = 0.7f,
            };

            try
            {
                var response = await _client.Models.GenerateContentAsync(_model, contents, config);
                return response.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini API 오류: {ex.Message}");
                return null;
            }
        }
    }
}
