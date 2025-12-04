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
        if (string.IsNullOrEmpty(_n8nSettings.Chat.GenerateWebhookUrl))
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
        var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n structured data webhook at {Url} with payload: {Payload}", _n8nSettings.Chat.GenerateWebhookUrl, jsonPayload);

            var response = await httpClient.PostAsync(_n8nSettings.Chat.GenerateWebhookUrl, content, cancellationToken);

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
                var deserializedResponse = JsonSerializer.Deserialize<T>(responseContent, options);
                if (deserializedResponse == null)
                {
                    _logger.LogWarning("Received empty or invalid structured data response from n8n. Raw: {RawResponse}", responseContent);
                    return Result<T>.Failure("Invalid response format from n8n: Empty or invalid structured data response.", "ExternalService");
                }
                return Result<T>.Success(deserializedResponse);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize n8n structured data webhook response as {Type}. Raw response: {RawResponse}", typeof(T).Name, responseContent);
                return Result<T>.Failure($"Invalid response format from n8n: Failed to deserialize structured data response as {typeof(T).Name}. Raw response: {responseContent}", "ExternalService");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n structured data webhook at {Url}.", _n8nSettings.Chat.GenerateWebhookUrl);
            return Result<T>.Failure($"An error occurred: {ex.Message}", "Exception");
        }
    }
}
