using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.Application.AI.DTOs;
using backend.Application.AI.DTOs.Embeddings;
using backend.Application.AI.Models;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.DTOs;
using backend.Infrastructure.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai dịch vụ để tương tác với n8n.
/// </summary>
public class N8nService : IN8nService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<N8nService> _logger;
    private readonly IJwtHelperFactory _jwtHelperFactory;
    public N8nService(IHttpClientFactory httpClientFactory, IOptions<N8nSettings> n8nSettings, ILogger<N8nService> logger, IJwtHelperFactory jwtHelperFactory)
    {
        _httpClientFactory = httpClientFactory;
        _n8nSettings = n8nSettings.Value;
        _logger = logger;
        _jwtHelperFactory = jwtHelperFactory;
    }
    public async Task<Result<string>> CallChatWebhookAsync(string sessionId, string message, CancellationToken cancellationToken)
    {
        var payload = new[]
        {
            new
            {
                sessionId,
                action = "sendMessage",
                chatInput = message
            }
        };
        return await CallN8nWebhookAsync<object, string>(
            _n8nSettings.Chat.ChatWebhookUrl,
            sessionId,
            payload,
            cancellationToken,
            responseParser: (responseContent) =>
            {
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    _logger.LogWarning("Received empty response content from n8n chat webhook.");
                    return Result<string>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
                }
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                try
                {
                    var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, options);
                    if (chatResponse != null && !string.IsNullOrEmpty(chatResponse.Output))
                    {
                        return Result<string>.Success(chatResponse.Output ?? string.Empty);
                    }
                    return Result<string>.Failure("Invalid response format from n8n: Empty or invalid chat response.", "ExternalService");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize n8n chat webhook response as ChatResponse. Raw response: {RawResponse}", responseContent);
                    return Result<string>.Failure($"Invalid response format from n8n: Failed to deserialize chat response. Raw response: {responseContent}", "ExternalService");
                }
            },
            logSuccessMessage: "Received from n8n",
            logFailureMessage: "Failed to call n8n chat webhook"
        );
    }
    public async Task<Result<ImageUploadResponseDto>> CallImageUploadWebhookAsync(ImageUploadWebhookDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_n8nSettings.Upload.WebHookUrl))
        {
            _logger.LogWarning("n8n image upload webhook URL is not configured.");
            return Result<ImageUploadResponseDto>.Failure("n8n image upload integration is not configured.", "Configuration");
        }
        var httpClient = _httpClientFactory.CreateClient();
        // Generate JWT Token if JwtSecret is configured
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            // Using a generic ID for image uploads if no specific entity ID is provided
            var tokenPayloadId = dto.FileName ?? Guid.NewGuid().ToString();
            var token = jwtHelper.GenerateToken(tokenPayloadId, DateTime.UtcNow.AddMinutes(5)); // Token expires in 5 minutes
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for image upload webhook.");
        }
        // Add image file
        using var content = new MultipartFormDataContent();
        var stream = new MemoryStream(dto.ImageData);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "data", dto.FileName!);
        var url = $"{_n8nSettings.Upload.WebHookUrl}?cloud={_n8nSettings.Upload.Cloud}&folder={dto.Folder}";
        try
        {
            _logger.LogInformation("Calling n8n image upload webhook at {Url}", url);
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n image upload webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<ImageUploadResponseDto>.Failure($"Failed to upload image via n8n webhook. Status: {response.StatusCode}", "ExternalService");
            }
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n image upload webhook: {ResponseContent}", responseContent);
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("Received empty response content from n8n image upload webhook.");
                return Result<ImageUploadResponseDto>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
            }
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            ImageUploadResponseDto? uploadResponse = null;
            try
            {
                uploadResponse = JsonSerializer.Deserialize<ImageUploadResponseDto>(responseContent, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize n8n image upload webhook response. Raw response: {RawResponse}", responseContent);
                return Result<ImageUploadResponseDto>.Failure($"Invalid response format from n8n: Failed to deserialize image upload response. Raw response: {responseContent}", "ExternalService");
            }
            if (uploadResponse == null)
            {
                _logger.LogWarning("Received empty or invalid image upload response from n8n.");
                return Result<ImageUploadResponseDto>.Failure("Invalid response format from n8n: Empty or invalid image upload response.", "ExternalService");
            }
            return Result<ImageUploadResponseDto>.Success(uploadResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n image upload webhook.");
            return Result<ImageUploadResponseDto>.Failure($"An error occurred: {ex.Message}", "Exception");
        }
    }
    public async Task<Result<FaceVectorOperationResultDto>> CallUpsertFaceVectorWebhookAsync(UpsertFaceVectorOperationDto dto, CancellationToken cancellationToken)
    {
        return await CallN8nWebhookAsync(
            _n8nSettings.Face.UpsertWebhookUrl,
            Guid.NewGuid().ToString(), // tokenPayloadId
            dto,
            cancellationToken,
            collectionName: _n8nSettings.Face.CollectionName,
            responseParser: ParseFaceVectorResponse,
            logSuccessMessage: "Received successful response from n8n upsert face vector webhook",
            logFailureMessage: "Failed to call n8n upsert face vector webhook"
        );
    }
    public async Task<Result<FaceVectorOperationResultDto>> CallSearchFaceVectorWebhookAsync(SearchFaceVectorOperationDto dto, CancellationToken cancellationToken)
    {
        return await CallN8nWebhookAsync(
            _n8nSettings.Face.SearchWebhookUrl,
            Guid.NewGuid().ToString(), // tokenPayloadId
            dto,
            cancellationToken,
            collectionName: _n8nSettings.Face.CollectionName,
            responseParser: ParseFaceVectorResponse,
            logSuccessMessage: "Received successful response from n8n search face vector webhook",
            logFailureMessage: "Failed to call n8n search face vector webhook"
        );
    }
    public async Task<Result<FaceVectorOperationResultDto>> CallDeleteFaceVectorWebhookAsync(DeleteFaceVectorOperationDto dto, CancellationToken cancellationToken)
    {
        return await CallN8nWebhookAsync(
            _n8nSettings.Face.DeleteWebhookUrl,
            dto.PointIds.FirstOrDefault() ?? Guid.NewGuid().ToString(), // tokenPayloadId
            dto,
            cancellationToken,
            collectionName: _n8nSettings.Face.CollectionName,
            responseParser: ParseFaceVectorResponse,
            logSuccessMessage: "Received successful response from n8n delete face vector webhook",
            logFailureMessage: "Failed to call n8n delete face vector webhook"
        );
    }
    private Result<FaceVectorOperationResultDto> ParseFaceVectorResponse(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            _logger.LogWarning("Received empty response content from n8n face vector webhook.");
            return Result<FaceVectorOperationResultDto>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
        }
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        N8nFaceVectorResponse? n8nResponse = null;
        try
        {
            n8nResponse = JsonSerializer.Deserialize<N8nFaceVectorResponse>(responseContent, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize n8n face vector webhook response as N8nFaceVectorResponse. Raw response: {RawResponse}", responseContent);
            return Result<FaceVectorOperationResultDto>.Failure($"Invalid response format from n8n: Failed to deserialize face vector operation response. Raw response: {responseContent}", "ExternalService");
        }
        if (n8nResponse == null)
        {
            _logger.LogWarning("Received empty or invalid N8nFaceVectorResponse from n8n.");
            return Result<FaceVectorOperationResultDto>.Failure("Invalid response format from n8n: Empty or invalid N8nFaceVectorResponse.", "ExternalService");
        }
        var operationResult = new FaceVectorOperationResultDto
        {
            Success = n8nResponse.Status == "ok",
            Message = n8nResponse.Status == "ok" ? "Operation successful" : "Operation failed",
            SearchResults = n8nResponse.Result?.Points?.Select(p => new FaceVectorSearchResultDto
            {
                Id = p.Id,
                Score = p.Score,
                Payload = p.Payload
            }).ToList(),
            AffectedCount = n8nResponse.Status == "ok" ? n8nResponse.Result?.Points?.Count ?? 0 : 0
        };
        return Result<FaceVectorOperationResultDto>.Success(operationResult);
    }
    private async Task<Result<TResponse>> CallN8nWebhookAsync<TRequest, TResponse>(
        string webhookUrl,
        string tokenPayloadId,
        TRequest dto,
        CancellationToken cancellationToken,
        string? collectionName = null,
        Func<string, Result<TResponse>>? responseParser = null,
        string logSuccessMessage = "Received successful response from n8n webhook",
        string logFailureMessage = "Failed to call n8n webhook")
        where TRequest : class
        where TResponse : class // Use class constraint for deserialization, string for direct return
    {
        if (string.IsNullOrEmpty(webhookUrl))
        {
            _logger.LogWarning("n8n webhook URL is not configured: {WebhookUrl}", webhookUrl);
            return Result<TResponse>.Failure($"n8n integration for {webhookUrl} is not configured.", "Configuration");
        }
        var httpClient = _httpClientFactory.CreateClient();
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            var token = jwtHelper.GenerateToken(tokenPayloadId, DateTime.UtcNow.AddMinutes(5));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for webhook.");
        }
        object payloadToSend;
        if (!string.IsNullOrEmpty(collectionName))
        {
            payloadToSend = new
            {
                CollectionName = collectionName,
                RequestData = dto
            };
        }
        else
        {
            payloadToSend = dto;
        }
        var jsonPayload = JsonSerializer.Serialize(payloadToSend, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        try
        {
            _logger.LogInformation("Calling n8n webhook at {Url} with payload: {Payload}", webhookUrl, jsonPayload);
            var response = await httpClient.PostAsync(webhookUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError($"{logFailureMessage}. Status: {{StatusCode}}, Response: {{ErrorContent}}", response.StatusCode, errorContent);
                return Result<TResponse>.Failure($"{logFailureMessage}. Status: {response.StatusCode}. Error: {errorContent}", "ExternalService");
            }
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation($"{logSuccessMessage}: {{ResponseContent}}", responseContent);
            if (responseParser != null)
            {
                return responseParser(responseContent);
            }
            else if (typeof(TResponse) == typeof(string))
            {
                return (Result<TResponse>)(object)Result<string>.Success(responseContent);
            }
            else
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
                try
                {
                    var deserializedResponse = JsonSerializer.Deserialize<TResponse>(responseContent, options);
                    if (deserializedResponse == null)
                    {
                        _logger.LogWarning("Received empty or invalid response from n8n. Raw: {RawResponse}", responseContent);
                        return Result<TResponse>.Failure("Invalid response format from n8n: Received empty or invalid response.", "ExternalService");
                    }
                    return Result<TResponse>.Success(deserializedResponse);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize n8n webhook response as {TResponse}. Raw: {RawResponse}", typeof(TResponse).Name, responseContent);
                    return Result<TResponse>.Failure($"Invalid response format from n8n: Failed to deserialize response as {typeof(TResponse).Name}. Raw: {responseContent}", "ExternalService");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n webhook at {Url}.", webhookUrl);
            return Result<TResponse>.Failure($"An error occurred: {ex.Message}", "Exception");
        }
    }
    public async Task<Result<string>> CallEmbeddingsWebhookAsync(BaseEmbeddingsDto dto, CancellationToken cancellationToken)
    {
        return await CallN8nWebhookAsync<BaseEmbeddingsDto, string>(
            _n8nSettings.Embeddings.WebHookUrl,
            dto.RecordId,
            dto,
            cancellationToken,
            _n8nSettings.Embeddings.CollectionName,
            logSuccessMessage: "Received successful response from n8n embeddings webhook",
            logFailureMessage: "Failed to call n8n embeddings webhook"
        );
    }
}
