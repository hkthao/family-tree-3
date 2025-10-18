using System.Net.Http.Headers;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Embeddings;

public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIEmbeddingProvider> _logger;

    public string ProviderName => "OpenAI";
    public int MaxTextLength => _settings.OpenAI.MaxTextLength;

    public OpenAIEmbeddingProvider(IOptions<EmbeddingSettings> embeddingSettings, HttpClient httpClient, ILogger<OpenAIEmbeddingProvider> logger)
    {
        _settings = embeddingSettings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<double[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenAI.ApiKey))
        {
            return Result<double[]>.Failure("OpenAI API key is not configured.");
        }

        if (text.Length > MaxTextLength)
        {
            text = text[..MaxTextLength];
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAI.ApiKey);
            var requestBody = new
            {
                model = _settings.OpenAI.Model,
                input = text
            };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAI.ApiKey);
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("data", out JsonElement dataElement) && dataElement.GetArrayLength() > 0)
                {
                    JsonElement embeddingElement = dataElement[0].GetProperty("embedding");
                    double[] embedding = embeddingElement.EnumerateArray().Select(e => e.GetDouble()).ToArray();
                    return Result<double[]>.Success(embedding);
                }
            }

            return Result<double[]>.Failure("Failed to parse embedding from OpenAI API response.");
        }
        catch (HttpRequestException ex)
        {
            return Result<double[]>.Failure($"OpenAI API request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<double[]>.Failure($"Failed to deserialize OpenAI API response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<double[]>.Failure($"An unexpected error occurred during OpenAI embedding generation: {ex.Message}");
        }
    }
}
