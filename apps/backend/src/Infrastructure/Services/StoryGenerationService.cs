using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;
using Microsoft.Extensions.Configuration; // Assuming Configuration is used for N8n settings
using System.Net.Http.Json; // For ReadFromJsonAsync
using System.Text.Json; // Added for JsonException

namespace backend.Infrastructure.Services;

public class StoryGenerationService : IStoryGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration; // For accessing n8n settings

    public StoryGenerationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Result<GenerateStoryResponseDto>> GenerateStoryAsync(GenerateStoryRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var n8nBaseUrl = _configuration["N8n:BaseUrl"];
            var n8nStoryGenerationWebhook = _configuration["N8n:StoryGenerationWebhook"];
            if (string.IsNullOrEmpty(n8nBaseUrl) || string.IsNullOrEmpty(n8nStoryGenerationWebhook))
            {
                return Result<GenerateStoryResponseDto>.Failure("N8n configuration for story generation is missing.");
            }

            var webhookUrl = $"{n8nBaseUrl}{n8nStoryGenerationWebhook}";

            var response = await _httpClient.PostAsJsonAsync(webhookUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadFromJsonAsync<GenerateStoryResponseDto>(cancellationToken: cancellationToken);

            if (responseBody == null)
            {
                return Result<GenerateStoryResponseDto>.Failure("N8n story generation returned empty response.");
            }

            return Result<GenerateStoryResponseDto>.Success(responseBody);
        }
        catch (HttpRequestException ex)
        {
            return Result<GenerateStoryResponseDto>.Failure($"N8n story generation request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<GenerateStoryResponseDto>.Failure($"Failed to parse n8n response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<GenerateStoryResponseDto>.Failure($"An unexpected error occurred during story generation: {ex.Message}");
        }
    }
}
