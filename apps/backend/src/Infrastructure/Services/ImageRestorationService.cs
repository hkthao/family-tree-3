using System.Net.Http.Json;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.ImageRestoration; // Added
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class ImageRestorationService : IImageRestorationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageRestorationService> _logger;
    private readonly ImageRestorationServiceSettings _settings;

    public ImageRestorationService(HttpClient httpClient, ILogger<ImageRestorationService> logger, IOptions<ImageRestorationServiceSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogWarning("ImageRestorationService BaseUrl is not configured. Ensure ImageRestorationServiceSettings:BaseUrl is set.");
        }
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl!);
    }

    public async Task<Result<StartImageRestorationResponseDto>> StartRestorationAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestDto = new StartImageRestorationRequestDto { ImageUrl = imageUrl };
            _logger.LogInformation("Sending StartImageRestorationRequest for imageUrl: {ImageUrl}", imageUrl);
            var response = await _httpClient.PostAsJsonAsync("/restore", requestDto, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StartImageRestorationResponseDto>(cancellationToken: cancellationToken);
            
            if (result == null)
            {
                _logger.LogError("Received null response from image restoration service when starting job.");
                return Result<StartImageRestorationResponseDto>.Failure("Null response from image restoration service.", "ImageRestorationService");
            }
            _logger.LogInformation("Image restoration job started with JobId: {JobId}, Status: {Status}", result.JobId, result.Status);
            return Result<StartImageRestorationResponseDto>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when starting image restoration for {ImageUrl}: {Message}", imageUrl, ex.Message);
            return Result<StartImageRestorationResponseDto>.Failure($"HTTP request failed: {ex.Message}", "ImageRestorationService");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed when starting image restoration for {ImageUrl}: {Message}", imageUrl, ex.Message);
            return Result<StartImageRestorationResponseDto>.Failure($"Invalid response format: {ex.Message}", "ImageRestorationService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred when starting image restoration for {ImageUrl}: {Message}", imageUrl, ex.Message);
            return Result<StartImageRestorationResponseDto>.Failure($"Unexpected error: {ex.Message}", "ImageRestorationService");
        }
    }

    public async Task<Result<ImageRestorationJobStatusDto>> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting image restoration job status for JobId: {JobId}", jobId);
            var response = await _httpClient.GetAsync($"/restore/{jobId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ImageRestorationJobStatusDto>(cancellationToken: cancellationToken);
            
            if (result == null)
            {
                _logger.LogError("Received null response from image restoration service when getting job status for JobId: {JobId}", jobId);
                return Result<ImageRestorationJobStatusDto>.Failure("Null response from image restoration service.", "ImageRestorationService");
            }
            _logger.LogInformation("Job status for JobId {JobId}: {Status}", jobId, result.Status);
            return Result<ImageRestorationJobStatusDto>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when getting job status for JobId {JobId}: {Message}", jobId, ex.Message);
            return Result<ImageRestorationJobStatusDto>.Failure($"HTTP request failed: {ex.Message}", "ImageRestorationService");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed when getting job status for JobId {JobId}: {Message}", jobId, ex.Message);
            return Result<ImageRestorationJobStatusDto>.Failure($"Invalid response format: {ex.Message}", "ImageRestorationService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred when getting job status for JobId {JobId}: {Message}", jobId, ex.Message);
            return Result<ImageRestorationJobStatusDto>.Failure($"Unexpected error: {ex.Message}", "ImageRestorationService");
        }
    }
}
