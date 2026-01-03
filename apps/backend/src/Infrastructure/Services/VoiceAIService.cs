using System.Net.Http.Json;
using System.Text.Json; // Added for JsonElement
using backend.Application.Common.Configurations;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Voice.DTOs;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class VoiceAIService : IVoiceAIService
{
    private readonly HttpClient _httpClient;
    private readonly VoiceAISettings _settings;

    public VoiceAIService(HttpClient httpClient, IOptions<VoiceAISettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            throw new ArgumentException("VoiceAISettings: BaseUrl is not configured.");
        }

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<Result<VoicePreprocessResponse>> PreprocessVoiceAsync(VoicePreprocessRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/voice/preprocess", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<VoicePreprocessResponse>.Failure($"Python Voice AI Service Preprocess failed with status {response.StatusCode}. Error: {errorContent}");
            }

            // Deserialize the JSON array response from Python
            // Python returns a tuple: (processed_audio_url, total_duration, quality_report)
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() != 3)
            {
                return Result<VoicePreprocessResponse>.Failure("Python Voice AI Service Preprocess returned an unexpected JSON format. Expected an array of 3 elements.");
            }

            var processedAudioUrl = root[0].GetString();
            var duration = root[1].GetDouble();
            var qualityReportJson = root[2]; // This is the JSON object for AudioQualityReport

            var qualityReportDto = JsonSerializer.Deserialize<AudioQualityReportDto>(qualityReportJson.ToString());

            if (processedAudioUrl == null || qualityReportDto == null)
            {
                return Result<VoicePreprocessResponse>.Failure("Python Voice AI Service Preprocess returned an invalid response content.");
            }

            var preprocessResponse = new VoicePreprocessResponse
            {
                ProcessedAudioUrl = processedAudioUrl,
                Duration = duration,
                QualityReport = qualityReportDto
            };
            
            return Result<VoicePreprocessResponse>.Success(preprocessResponse);
        }
        catch (HttpRequestException ex)
        {
            return Result<VoicePreprocessResponse>.Failure($"Network error connecting to Python Voice AI Service Preprocess: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<VoicePreprocessResponse>.Failure($"An unexpected error occurred during Python Voice AI Service Preprocess: {ex.Message}");
        }
    }

    public async Task<Result<VoiceGenerateResponse>> GenerateVoiceAsync(VoiceGenerateRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/voice/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<VoiceGenerateResponse>.Failure($"Python Voice AI Service Generate failed with status {response.StatusCode}. Error: {errorContent}");
            }

            var generateResponse = await response.Content.ReadFromJsonAsync<VoiceGenerateResponse>();
            if (generateResponse == null)
            {
                return Result<VoiceGenerateResponse>.Failure("Python Voice AI Service Generate returned empty response.");
            }

            return Result<VoiceGenerateResponse>.Success(generateResponse);
        }
        catch (HttpRequestException ex)
        {
            return Result<VoiceGenerateResponse>.Failure($"Network error connecting to Python Voice AI Service Generate: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<VoiceGenerateResponse>.Failure($"An unexpected error occurred during Python Voice AI Service Generate: {ex.Message}");
        }
    }
}
