using System.Text;
using System.Text.Json;
using backend.Application.Common.Interfaces.Services.LLMGateway;
using backend.Application.Common.Models; // Add this using directive for Result
using backend.Application.Common.Models.LLMGateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Add this

namespace backend.Infrastructure.Services.LLMGateway;

public class LLMGatewayService : ILLMGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly string _llmGatewayBaseUrl;
    private readonly ILogger<LLMGatewayService> _logger; // Add logger field

    public LLMGatewayService(HttpClient httpClient, IConfiguration configuration, ILogger<LLMGatewayService> logger) // Add logger to constructor
    {
        _httpClient = httpClient;
        _llmGatewayBaseUrl = configuration["LLMGatewayService:BaseUrl"] ?? throw new ArgumentNullException("LLMGatewayService:BaseUrl is not configured.");
        _httpClient.BaseAddress = new Uri(_llmGatewayBaseUrl);
        _logger = logger; // Assign logger
    }

    public async Task<Result<LLMChatCompletionResponse>> GetChatCompletionAsync(
        LLMChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestBody = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<LLMChatCompletionResponse>.Failure($"LLM Gateway Chat API call failed with status {response.StatusCode}: {errorContent}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var chatCompletionResponse = JsonSerializer.Deserialize<LLMChatCompletionResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (chatCompletionResponse == null)
        {
            return Result<LLMChatCompletionResponse>.Failure("Failed to deserialize LLM Gateway Chat API response.");
        }

        return Result<LLMChatCompletionResponse>.Success(chatCompletionResponse);
    }

    public async Task<Result<LLMEmbeddingResponse>> GetEmbeddingsAsync(
        LLMEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestBody = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/embeddings", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<LLMEmbeddingResponse>.Failure($"LLM Gateway Embeddings API call failed with status {response.StatusCode}: {errorContent}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var embeddingResponse = JsonSerializer.Deserialize<LLMEmbeddingResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (embeddingResponse == null)
        {
            return Result<LLMEmbeddingResponse>.Failure("Failed to deserialize LLM Gateway Embeddings API response.");
        }

        return Result<LLMEmbeddingResponse>.Success(embeddingResponse);
    }
}
