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
/// Dịch vụ để gọi webhook chat AI của n8n.
/// </summary>
public class AiChatService : IAiChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<AiChatService> _logger;
    private readonly IJwtHelperFactory _jwtHelperFactory;

    public AiChatService(
        IHttpClientFactory httpClientFactory,
        IOptions<N8nSettings> n8nSettings,
        ILogger<AiChatService> logger,
        IJwtHelperFactory jwtHelperFactory)
    {
        _httpClientFactory = httpClientFactory;
        _n8nSettings = n8nSettings.Value;
        _logger = logger;
        _jwtHelperFactory = jwtHelperFactory;
    }

    /// <summary>
    /// Gọi webhook chat của n8n để gửi tin nhắn và nhận phản hồi từ AI.
    /// </summary>
    /// <param name="sessionId">ID của phiên chat.</param>
    /// <param name="message">Tin nhắn từ người dùng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa phản hồi từ AI.</returns>
    public async Task<Result<ChatResponse>> CallChatWebhookAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_n8nSettings.Chat.ChatWebhookUrl))
        {
            _logger.LogWarning("n8n chat webhook URL is not configured.");
            return Result<ChatResponse>.Failure("n8n chat integration is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        // Generate JWT Token if JwtSecret is configured
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            var token = jwtHelper.GenerateToken(request.SessionId, DateTime.UtcNow.AddMinutes(5)); // Token expires in 5 minutes
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for chat webhook.");
        }

        if (!string.IsNullOrEmpty(_n8nSettings.Chat.CollectionName))
        {
            request.Metadata.Add("collectionName", _n8nSettings.Chat.CollectionName);
        }
        else
        {
            _logger.LogWarning("N8nSettings.Chat.CollectionName is not configured. Skipping adding collectionName to metadata.");
        }

        var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n chat webhook at {Url} with payload: {Payload}", _n8nSettings.Chat.ChatWebhookUrl, jsonPayload);

            var response = await httpClient.PostAsync(_n8nSettings.Chat.ChatWebhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n chat webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<ChatResponse>.Failure($"Failed to call n8n chat webhook. Status: {response.StatusCode}. Error: {errorContent}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n chat webhook: {ResponseContent}", responseContent);

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("Received empty response content from n8n chat webhook.");
                return Result<ChatResponse>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, options);
                if (chatResponse != null && !string.IsNullOrEmpty(chatResponse.Output))
                {
                    return Result<ChatResponse>.Success(chatResponse);
                }
                return Result<ChatResponse>.Failure("Invalid response format from n8n: Empty or invalid chat response.", "ExternalService");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize n8n chat webhook response as ChatResponse. Raw response: {RawResponse}", responseContent);
                return Result<ChatResponse>.Failure($"Invalid response format from n8n: Failed to deserialize chat response. Raw response: {responseContent}", "ExternalService");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n chat webhook at {Url}.", _n8nSettings.Chat.ChatWebhookUrl);
            return Result<ChatResponse>.Failure($"An error occurred: {ex.Message}", "Exception");
        }
    }
}
