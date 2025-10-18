using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Commands;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;

namespace backend.Infrastructure.Services;

public class FaceApiService : IFaceApiService
{
    private readonly ILogger<FaceApiService> _logger;
    private readonly HttpClient _httpClient;

    public FaceApiService(ILogger<FaceApiService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Result<List<float>>> GetFaceEmbeddingAsync(string base64Image, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calling Python service for face embedding.");

        try
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(new { image = base64Image }), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/get_embedding", requestContent, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var embedding = JsonSerializer.Deserialize<List<float>>(responseContent);

            if (embedding == null)
            {
                return Result<List<float>>.Failure("Failed to deserialize embedding from Python service.");
            }

            return Result<List<float>>.Success(embedding);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling Python service for face embedding.");
            return Result<List<float>>.Failure($"Error calling Python service: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing embedding from Python service response.");
            return Result<List<float>>.Failure($"Error deserializing response: {ex.Message}");
        }
    }

    public async Task<List<FaceDetectionResultDto>> DetectFacesAsync(byte[] imageBytes, string contentType, bool returnCrop)
    {
          _logger.LogInformation("Calling Python Face Detection Service for face detection.");

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", "image.jpg"); // "image.jpg" is a placeholder filename

        var requestUri = $"/detect?return_crop={returnCrop}";
        var response = await _httpClient.PostAsync(requestUri, form);

        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        jsonContent= jsonContent.Replace("bounding_box", "boundingBox");
       // _logger.LogInformation($"Raw JSON from Python Face Detection Service: {jsonContent}");

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };
        var result = await System.Text.Json.JsonSerializer.DeserializeAsync<List<FaceDetectionResultDto>>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent)), options);
        //_logger.LogInformation($"Deserialized FaceDetectionResultDto in C#: {System.Text.Json.JsonSerializer.Serialize(result)}");
        _logger.LogInformation("Successfully received response from Python Face Detection Service.");

        return result ?? new List<FaceDetectionResultDto>();
    }
}
