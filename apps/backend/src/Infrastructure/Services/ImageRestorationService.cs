using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.ImageRestoration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class ImageRestorationService : IImageRestorationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageRestorationService> _logger;
    private readonly ImageRestorationServiceSettings _settings;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ImageRestorationService(HttpClient httpClient, ILogger<ImageRestorationService> logger, IOptions<ImageRestorationServiceSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        // Configure JsonSerializerOptions to handle enum as strings
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // To handle camelCase properties from API
            Converters = { new JsonStringEnumConverter() } // To convert enum strings (e.g., "Processing")
        };

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogWarning("ImageRestorationService BaseUrl is not configured. Ensure ImageRestorationServiceSettings:BaseUrl is set.");
        }
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl!);
    }

    public async Task<Result<StartImageRestorationResponseDto>> StartRestorationAsync(string imageUrl, bool useCodeformer, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestDto = new StartImageRestorationRequestDto { ImageUrl = imageUrl, UseCodeformer = useCodeformer };
            _logger.LogInformation("Sending StartImageRestorationRequest for imageUrl: {ImageUrl}, useCodeformer: {UseCodeformer}", imageUrl, useCodeformer);
            var response = await _httpClient.PostAsJsonAsync("/restore", requestDto, cancellationToken);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received JSON response from image restoration service: {JsonResponse}", jsonResponse);

            var result = JsonSerializer.Deserialize<StartImageRestorationResponseDto>(jsonResponse, _jsonSerializerOptions);
            
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

            var result = await response.Content.ReadFromJsonAsync<ImageRestorationJobStatusDto>(_jsonSerializerOptions, cancellationToken);
            
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

    public async Task<Result<PreprocessImageResponseDto>> PreprocessImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending image for preprocessing: {FileName}", fileName);

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(imageStream);
            streamContent.Headers.Add("Content-Type", contentType);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("/preprocess-image", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadFromJsonAsync<PreprocessImageResponseDto>(_jsonSerializerOptions, cancellationToken);

            if (responseContent == null || string.IsNullOrEmpty(responseContent.ProcessedImageBase64))
            {
                _logger.LogError("Received invalid response from image preprocessing service.");
                return Result<PreprocessImageResponseDto>.Failure("Invalid response from image preprocessing service.", "ImageRestorationService");
            }

            _logger.LogInformation("Image preprocessing successful. Received base64 image data. Image resized: {IsResized}", responseContent.IsResized);
            return Result<PreprocessImageResponseDto>.Success(responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during image preprocessing for {FileName}: {Message}", fileName, ex.Message);
            return Result<PreprocessImageResponseDto>.Failure($"HTTP request failed: {ex.Message}", "ImageRestorationService");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed during image preprocessing for {FileName}: {Message}", fileName, ex.Message);
            return Result<PreprocessImageResponseDto>.Failure($"Invalid response format: {ex.Message}", "ImageRestorationService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during image preprocessing for {FileName}: {Message}", fileName, ex.Message);
            return Result<PreprocessImageResponseDto>.Failure($"Unexpected error: {ex.Message}", "ImageRestorationService");
        }
    }
}
