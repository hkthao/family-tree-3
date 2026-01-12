using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.Application.AI.DTOs;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Infrastructure.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

/// <summary>
/// Dịch vụ để gọi webhook n8n cho việc chuyển đổi văn bản thành dữ liệu có cấu trúc.
/// </summary>
public class AiGenerateService : IAiGenerateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<AiGenerateService> _logger;
    private readonly IJwtHelperFactory _jwtHelperFactory;

    public AiGenerateService(
        IHttpClientFactory httpClientFactory,
        IOptions<N8nSettings> n8nSettings,
        ILogger<AiGenerateService> logger,
        IJwtHelperFactory jwtHelperFactory)
    {
        _httpClientFactory = httpClientFactory;
        _n8nSettings = n8nSettings.Value;
        _logger = logger;
        _jwtHelperFactory = jwtHelperFactory;
    }

    /// <summary>
    /// Gọi webhook n8n để yêu cầu AI Agent chuyển đổi văn bản thành dữ liệu có cấu trúc.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu mong muốn được trả về sau khi phân tích.</typeparam>
    /// <param name="request">Yêu cầu chứa ChatInput, SystemPrompt và Metadata.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa dữ liệu có cấu trúc đã phân tích.</returns>
    public async Task<Result<T>> GenerateDataAsync<T>(GenerateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_n8nSettings.Chat.ChatWebhookUrl))
        {
            _logger.LogWarning("n8n structured data webhook URL is not configured.");
            return Result<T>.Failure("n8n structured data integration is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        // Generate JWT Token if JwtSecret is configured
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            // Use SessionId from the request for JWT token generation
            var token = jwtHelper.GenerateToken(request.SessionId, DateTime.UtcNow.AddMinutes(5)); // Token expires in 5 minutes
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for structured data webhook.");
        }

        // The request object itself contains all necessary data including ChatInput, SystemPrompt, and Metadata
        // We can directly serialize the request object as the payload for the webhook
        request.Context = Application.AI.Enums.ContextType.DataGeneration;
        var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n structured data webhook at {Url} with payload: {Payload}", _n8nSettings.Chat.ChatWebhookUrl, jsonPayload);

            var response = await httpClient.PostAsync(_n8nSettings.Chat.ChatWebhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n structured data webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<T>.Failure($"Failed to call n8n structured data webhook. Status: {response.StatusCode}. Error: {errorContent}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n structured data webhook: {ResponseContent}", responseContent);

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("Received empty response content from n8n structured data webhook.");
                return Result<T>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                // Deserialize the initial response into ChatResponse
                var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, options);

                if (chatResponse == null || string.IsNullOrWhiteSpace(chatResponse.Output))
                {
                    _logger.LogWarning("Received empty or invalid ChatResponse or its Output from n8n. Raw: {RawResponse}", responseContent);
                    return Result<T>.Failure("Invalid response format from n8n: Empty or invalid ChatResponse or its Output.", "ExternalService");
                }

                T? deserializedOutput;
                try
                {
                    // Attempt to deserialize chatResponse.Output as a JSON string for type T
                    deserializedOutput = JsonSerializer.Deserialize<T>(chatResponse.Output, options);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize ChatResponse.Output as JSON for type {TypeName}. Attempting direct assignment for RelationshipInferenceResultDto if applicable. Raw output: {RawOutput}", typeof(T).Name, chatResponse.Output);

                    // If deserialization fails and T is RelationshipInferenceResultDto,
                    // assume chatResponse.Output is the raw string for InferredRelationship.
                    if (typeof(T) == typeof(RelationshipInferenceResultDto))
                    {
                        var resultDto = new RelationshipInferenceResultDto
                        {
                            InferredRelationship = chatResponse.Output
                        };
                        deserializedOutput = (T)(object)resultDto; // Cast to T
                    }
                    else if (typeof(T) == typeof(CombinedAiContentDto)) // Handle raw output for CombinedAiContentDto
                    {
                        _logger.LogWarning("Failed to deserialize ChatResponse.Output as JSON for CombinedAiContentDto. Storing raw output. Raw output: {RawOutput}", chatResponse.Output);
                        var combinedAiContentDto = new CombinedAiContentDto
                        {
                            RawOutput = chatResponse.Output
                        };
                        deserializedOutput = (T)(object)combinedAiContentDto; // Cast to T
                    }
                    else
                    {
                        // If it's not RelationshipInferenceResultDto or another unexpected error, rethrow
                        return Result<T>.Failure($"Failed to deserialize structured data from ChatResponse.Output. Expected JSON for type {typeof(T).Name}.", "ExternalService");
                    }
                }

                if (deserializedOutput == null)
                {
                    _logger.LogWarning("Received empty or invalid structured data from ChatResponse.Output. Raw: {RawOutput}", chatResponse.Output);
                    return Result<T>.Failure("Invalid response format from n8n: Empty or invalid structured data from ChatResponse.Output.", "ExternalService");
                }
                return Result<T>.Success(deserializedOutput);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize n8n structured data webhook response. Raw response: {RawResponse}", responseContent);
                return Result<T>.Failure($"Invalid response format from n8n: Failed to deserialize response. Raw response: {responseContent}", "ExternalService");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n structured data webhook at {Url}.", _n8nSettings.Chat.ChatWebhookUrl);
            return Result<T>.Failure($"An error occurred: {ex.Message}", "Exception");
        }
    }
}
