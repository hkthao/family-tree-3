using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Embeddings;

public class LocalEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalEmbeddingProvider> _logger;

    public string ProviderName => "Local";
    public int MaxTextLength => _settings.Local.MaxTextLength;

    public LocalEmbeddingProvider(IOptions<EmbeddingSettings> embeddingSettings, HttpClient httpClient, ILogger<LocalEmbeddingProvider> logger)
    {
        _settings = embeddingSettings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<double[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<double[]>.Failure("Text for embedding cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Local.ApiUrl))
        {
            return Result<double[]>.Failure("Ollama API URL is not configured.");
        }
        if (string.IsNullOrWhiteSpace(_settings.Local.Model))
        {
            return Result<double[]>.Failure("Ollama model is not configured.");
        }

        if (text.Length > MaxTextLength)
        {
            text = text[..MaxTextLength];
        }

        try
        {
            var requestBody = new
            {
                model = _settings.Local.Model,
                input = text
            };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_settings.Local.ApiUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("embeddings", out JsonElement embeddingsElement) && embeddingsElement.GetArrayLength() > 0)
                {
                    JsonElement embeddingElement = embeddingsElement[0]; // Get the first embedding from the array
                    double[] embedding = embeddingElement.EnumerateArray().Select(e => e.GetDouble()).ToArray();
                    _logger.LogInformation("Generated local embedding with dimension: {Dimension}", embedding.Length);
                    return Result<double[]>.Success(embedding);
                }
            }

            return Result<double[]>.Failure("Failed to parse embedding from Ollama API response.");
        }
        catch (HttpRequestException ex)
        {
            return Result<double[]>.Failure($"Ollama API request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<double[]>.Failure($"Failed to deserialize Ollama API response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<double[]>.Failure($"An unexpected error occurred during Ollama embedding generation: {ex.Message}");
        }
    }
}
