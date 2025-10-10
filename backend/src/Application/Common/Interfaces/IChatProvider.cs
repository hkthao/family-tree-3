namespace backend.Application.Common.Interfaces;

public interface IChatProvider
{
    Task<string> GenerateResponseAsync(string prompt);
}
