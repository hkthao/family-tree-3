namespace backend.Application.Common.Interfaces;

public interface ILLMProviderFactory
{
    ILLMProvider GetProvider();
}
