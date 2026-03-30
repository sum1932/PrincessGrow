using System.Threading.Tasks;

namespace DessertKingdom.TestConsole
{
    public interface ILLMClient
    {
        Task<string> GetCompletionAsync(string prompt, int maxTokens = 500);
    }
}
