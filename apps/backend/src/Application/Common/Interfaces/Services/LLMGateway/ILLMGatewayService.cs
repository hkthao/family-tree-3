using backend.Application.Common.Models;
using backend.Application.Common.Models.LLMGateway;

namespace backend.Application.Common.Interfaces.Services.LLMGateway;

public interface ILLMGatewayService
{
    Task<Result<LLMChatCompletionResponse>> GetChatCompletionAsync(
        LLMChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<LLMEmbeddingResponse>> GetEmbeddingsAsync(
        LLMEmbeddingRequest request,
        CancellationToken cancellationToken = default);
}
