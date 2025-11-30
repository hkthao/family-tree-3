using System.Net.Http.Headers; // For MediaTypeHeaderValue
using System.Text; // NEW USING
using System.Text.Json; // NEW USING
using backend.Application.AI.DTOs; // UPDATED USING
using backend.Application.AI.Models;
using backend.Application.Common.Interfaces; // NEW USING
using backend.Application.Common.Models;
using backend.Application.Common.Models.AI;
using backend.Application.Common.Models.AppSetting;
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

    /// <inheritdoc />
    public async Task<Result<string>> CallChatWebhookAsync(string sessionId, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_n8nSettings.ChatWebhookUrl) || _n8nSettings.ChatWebhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n chat webhook URL is not configured.");
            return Result<string>.Failure("n8n chat integration is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        // Generate JWT Token if JwtSecret is configured
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            var token = jwtHelper.GenerateToken(sessionId, DateTime.UtcNow.AddMinutes(5)); // Token expires in 5 minutes
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for chat webhook.");
        }

        var payload = new[]
        {
            new
            {
                sessionId,
                action = "sendMessage",
                chatInput = message
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n chat webhook at {Url}", _n8nSettings.ChatWebhookUrl);
            var response = await httpClient.PostAsync(_n8nSettings.ChatWebhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Failed to get response from AI assistant. Status: {response.StatusCode}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n: {ResponseContent}", responseContent);
            _logger.LogDebug("Raw n8n chat webhook response: {ResponseContent}", responseContent); // Log raw response
            _logger.LogInformation("Received successful response from n8n webhook.");
            _logger.LogDebug("Raw n8n chat webhook response: {ResponseContent}", responseContent); // Log raw response

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("Received empty response content from n8n chat webhook.");
                return Result<string>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            ChatResponse? chatResponse = null;

            try
            {
                chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize n8n chat webhook response as ChatResponse. Raw response: {RawResponse}", responseContent);
                return Result<string>.Failure($"Invalid response format from n8n: Failed to deserialize chat response. Raw response: {responseContent}", "ExternalService");
            }

            if (chatResponse != null && !string.IsNullOrEmpty(chatResponse.Output))
            {
                return Result<string>.Success(chatResponse.Output ?? string.Empty);
            }

            return Result<string>.Failure("Invalid response format from n8n: Empty or invalid chat response.", "ExternalService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n webhook.");
            return Result<string>.Failure("An error occurred: {ex.Message}", "Exception");
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> CallEmbeddingWebhookAsync(EmbeddingWebhookDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_n8nSettings.EmbeddingWebhookUrl) || _n8nSettings.EmbeddingWebhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n embedding webhook URL is not configured.");
            return Result<string>.Failure("n8n embedding integration is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        // Generate JWT Token if JwtSecret is configured
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            var token = jwtHelper.GenerateToken(dto.EntityId, DateTime.UtcNow.AddMinutes(5)); // Token expires in 5 minutes
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for embedding webhook.");
        }

        var payload = new
        {
            dto.EntityType,
            dto.EntityId,
            dto.ActionType,
            dto.EntityData,
            dto.Description,
            _n8nSettings.Face.CollectionName // Add CollectionName to payload
        };

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n embedding webhook at {Url} with payload: {Payload}", _n8nSettings.EmbeddingWebhookUrl, jsonPayload);
            var response = await httpClient.PostAsync(_n8nSettings.EmbeddingWebhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n embedding webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Failed to trigger n8n embedding workflow. Status: {response.StatusCode}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n embedding webhook: {ResponseContent}", responseContent);

            // Assuming n8n returns a JSON object with a 'result' property, which contains a 'points' array,
            // and each point has a 'payload' with a 'member_id' or 'memberId' property.
            var jsonDocument = JsonDocument.Parse(responseContent);
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (jsonDocument.RootElement.TryGetProperty("result", out var resultElement) && resultElement.TryGetProperty("points", out var pointsElement) && pointsElement.ValueKind == JsonValueKind.Array && pointsElement.GetArrayLength() > 0)
                {
                    var firstPoint = pointsElement[0];
                    if (firstPoint.TryGetProperty("payload", out var payloadElement))
                    {
                        // Try to get "member_id" (snake_case) first, then "memberId" (camelCase)
                        if (payloadElement.TryGetProperty("member_id", out var memberIdElement) && memberIdElement.ValueKind == JsonValueKind.String)
                        {
                            var memberId = memberIdElement.GetString();
                            _logger.LogInformation("Successfully received memberId '{MemberId}' from n8n for {EntityType} {EntityId} ({ActionType}).", memberId, dto.EntityType, dto.EntityId, dto.ActionType);
                            return Result<string>.Success(memberId ?? string.Empty);
                        }
                        else if (payloadElement.TryGetProperty("memberId", out memberIdElement) && memberIdElement.ValueKind == JsonValueKind.String)
                        {
                            var memberId = memberIdElement.GetString();
                            _logger.LogInformation("Successfully received memberId '{MemberId}' from n8n for {EntityType} {EntityId} ({ActionType}).", memberId, dto.EntityType, dto.EntityId, dto.ActionType);
                            return Result<string>.Success(memberId ?? string.Empty);
                        }
                    }
                }
            }

            _logger.LogInformation("No memberId found or invalid format in n8n embedding webhook response for {EntityType} {EntityId} ({ActionType}).", dto.EntityType, dto.EntityId, dto.ActionType);
            return Result<string>.Success(string.Empty); // No member found, return empty string
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n embedding webhook.");
            return Result<string>.Failure($"An error occurred while triggering n8n embedding workflow: {ex.Message}", "Exception");
        }
    }

    /// <inheritdoc />
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


    /// <inheritdoc />
    public async Task<Result<FaceVectorOperationResultDto>> CallUpsertFaceVectorWebhookAsync(UpsertFaceVectorOperationDto dto, CancellationToken cancellationToken)
    {
        return await CallFaceVectorWebhookInternalAsync(
            _n8nSettings.Face.UpsertWebhookUrl,
            dto.Payload != null && dto.Payload.TryGetValue("localDbId", out var idValue) && idValue != null
                ? idValue.ToString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString(),
            dto,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<Result<FaceVectorOperationResultDto>> CallSearchFaceVectorWebhookAsync(SearchFaceVectorOperationDto dto, CancellationToken cancellationToken)
    {
        return await CallFaceVectorWebhookInternalAsync(
            _n8nSettings.Face.SearchWebhookUrl,
            dto.Filter != null && dto.Filter.TryGetValue("memberId", out var idValue) && idValue != null
                ? idValue.ToString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString(),
            dto,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<Result<FaceVectorOperationResultDto>> CallDeleteFaceVectorWebhookAsync(DeleteFaceVectorOperationDto dto, CancellationToken cancellationToken)
    {
        return await CallFaceVectorWebhookInternalAsync(
            _n8nSettings.Face.DeleteWebhookUrl,
            dto.PointIds.FirstOrDefault() ?? Guid.NewGuid().ToString(),
            dto,
            cancellationToken
        );
    }

    private async Task<Result<FaceVectorOperationResultDto>> CallFaceVectorWebhookInternalAsync(string webhookUrl, string tokenPayloadId, object dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(webhookUrl) || webhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n face vector webhook URL is not configured: {WebhookUrl}", webhookUrl);
            return Result<FaceVectorOperationResultDto>.Failure($"n8n face vector integration for {webhookUrl} is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        // Generate JWT Token if JwtSecret is configured
        if (!string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            var jwtHelper = _jwtHelperFactory.Create(_n8nSettings.JwtSecret);
            var token = jwtHelper.GenerateToken(tokenPayloadId, DateTime.UtcNow.AddMinutes(5)); // Token expires in 5 minutes
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("N8nSettings.JwtSecret is not configured. Skipping JWT token generation for face vector webhook.");
        }

        // Include CollectionName in the payload that is sent to n8n
        var payloadWithCollection = new
        {
            CollectionName = _n8nSettings.Face.CollectionName,
            RequestData = dto // The specific DTO for the operation
        };

        var jsonPayload = JsonSerializer.Serialize(payloadWithCollection, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n face vector webhook at {Url} with payload: {Payload}", webhookUrl, jsonPayload);
            var response = await httpClient.PostAsync(webhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n face vector webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<FaceVectorOperationResultDto>.Failure($"Failed to perform face vector operation via n8n webhook. Status: {response.StatusCode}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n face vector webhook: {ResponseContent}", responseContent);

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
                return Result<FaceVectorOperationResultDto>.Failure("Invalid response format from n8n: Empty or invalid face vector operation response.", "ExternalService");
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
                AffectedCount = n8nResponse.Status == "ok" ? n8nResponse.Result?.Points?.Count ?? 0 : 0 // Assuming affected count is number of points for successful operations
            };

            return Result<FaceVectorOperationResultDto>.Success(operationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n face vector webhook.");
            return Result<FaceVectorOperationResultDto>.Failure($"An error occurred: {ex.Message}", "Exception");
        }
    }


}
