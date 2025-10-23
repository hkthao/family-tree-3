using System.Net.Http.Headers;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.AI.Embeddings;

public class OpenAIEmbeddingProvider(IConfigProvider configProvider, HttpClient httpClient, ILogger<OpenAIEmbeddingProvider> logger) : IEmbeddingProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<OpenAIEmbeddingProvider> _logger = logger;
    private readonly IConfigProvider _configProvider = configProvider;

    public string ProviderName => "OpenAI";
    public int MaxTextLength => _configProvider.GetSection<EmbeddingSettings>().OpenAI.MaxTextLength;

    public async Task<Result<double[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var settings = _configProvider.GetSection<EmbeddingSettings>();
        if (string.IsNullOrWhiteSpace(settings.OpenAI.ApiKey))
        {
            return Result<double[]>.Failure("OpenAI API key is not configured.");
        }

        if (text.Length > MaxTextLength)
        {
            text = text[..MaxTextLength];
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.OpenAI.ApiKey);
            var requestBody = new
            {
                model = settings.OpenAI.Model,
                input = text
            };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.OpenAI.ApiKey);
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
