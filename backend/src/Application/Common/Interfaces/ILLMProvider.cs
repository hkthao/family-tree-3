namespace backend.Application.Common.Interfaces;

public interface ILLMProvider
{
    Task<string> GenerateResponseAsync(string prompt);
}
