using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;
using Microsoft.Extensions.Configuration; // Assuming Configuration is used for N8n settings
using System.Text.Json; // For JsonDocument
using System.Net.Http.Json; // For ReadFromJsonAsync

namespace backend.Infrastructure.Services;

public class PhotoAnalysisService : IPhotoAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration; // For accessing n8n settings

    public PhotoAnalysisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Result<PhotoAnalysisResultDto>> AnalyzePhotoAsync(AnalyzePhotoRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var n8nBaseUrl = _configuration["N8n:BaseUrl"];
            var n8nPhotoAnalysisWebhook = _configuration["N8n:PhotoAnalysisWebhook"];
            if (string.IsNullOrEmpty(n8nBaseUrl) || string.IsNullOrEmpty(n8nPhotoAnalysisWebhook))
            {
                return Result<PhotoAnalysisResultDto>.Failure("N8n configuration for photo analysis is missing.");
            }

            var webhookUrl = $"{n8nBaseUrl}{n8nPhotoAnalysisWebhook}";

            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(request.File.OpenReadStream()), "file", request.File.FileName);
            if (request.MemberId.HasValue)
            {
                form.Add(new StringContent(request.MemberId.Value.ToString()), "memberId");
            }

            var response = await _httpClient.PostAsync(webhookUrl, form, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadFromJsonAsync<PhotoAnalysisResultDto>(cancellationToken: cancellationToken);

            if (responseBody == null)
            {
                return Result<PhotoAnalysisResultDto>.Failure("N8n photo analysis returned empty response.");
            }

            return Result<PhotoAnalysisResultDto>.Success(responseBody);
        }
        catch (HttpRequestException ex)
        {
            return Result<PhotoAnalysisResultDto>.Failure($"N8n photo analysis request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<PhotoAnalysisResultDto>.Failure($"Failed to parse n8n response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<PhotoAnalysisResultDto>.Failure($"An unexpected error occurred during photo analysis: {ex.Message}");
        }
    }
}
