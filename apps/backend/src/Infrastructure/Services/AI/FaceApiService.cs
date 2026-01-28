using System.Net.Http.Headers;
using System.Collections.Generic;
using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Common;
using Microsoft.Extensions.Logging; // NEW: Add this using for FaceDetectionResultDto

namespace backend.Infrastructure.Services;

public class FaceApiService(ILogger<FaceApiService> logger, HttpClient httpClient) : IFaceApiService
{
    private readonly ILogger<FaceApiService> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;

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
        jsonContent = jsonContent.Replace("bounding_box", "boundingBox");
        // _logger.LogInformation($"Raw JSON from Python Face Detection Service: {jsonContent}");

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };
        var result = await System.Text.Json.JsonSerializer.DeserializeAsync<List<FaceDetectionResultDto>>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent)), options);
        //_logger.LogInformation($"Deserialized FaceDetectionResultDto in C#: {System.Text.Json.JsonSerializer.Serialize(result)}");
        _logger.LogInformation("Successfully received response from Python Face Detection Service.");

        return result ?? [];
    }

    public async Task<byte[]> ResizeImageAsync(byte[] imageBytes, string contentType, int width, int? height = null)
    {
        _logger.LogInformation("Calling Python Face Detection Service for image resizing.");

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", "image.jpg"); // "image.jpg" is a placeholder filename

        var requestUri = $"/resize?width={width}";
        if (height.HasValue)
        {
            requestUri += $"&height={height.Value}";
        }

        var response = await _httpClient.PostAsync(requestUri, form);

        response.EnsureSuccessStatusCode();

        var resizedImageBytes = await response.Content.ReadAsByteArrayAsync();
        _logger.LogInformation("Successfully received resized image from Python Face Detection Service.");

        return resizedImageBytes;
    }

    public async Task<Dictionary<string, object>> AddFaceWithMetadataAsync(byte[] imageBytes, string contentType, FaceMetadataDto metadata)
    {
        _logger.LogInformation("Calling Python Face Detection Service to add face with metadata.");

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", "image.jpg");

        // Serialize metadata to JSON string
        var metadataJson = System.Text.Json.JsonSerializer.Serialize(metadata);
        var metadataContent = new StringContent(metadataJson);
        metadataContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        form.Add(metadataContent, "metadata");

        var response = await _httpClient.PostAsync("/faces", form);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, options);

        _logger.LogInformation("Successfully added face with metadata.");
        return result ?? new Dictionary<string, object>();
    }

    public async Task<Dictionary<string, object>> AddFaceByVectorAsync(FaceAddVectorRequestDto request)
    {
        _logger.LogInformation("Calling Python Face Detection Service to add face by vector.");

        var requestUri = "/faces/vector";
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var jsonContent = System.Text.Json.JsonSerializer.Serialize(request, options);
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUri, httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson, options);

        _logger.LogInformation("Successfully added face by vector.");
        return result ?? new Dictionary<string, object>();
    }

    public async Task<List<Dictionary<string, object>>> GetFacesByFamilyIdAsync(string familyId)
    {
        _logger.LogInformation($"Calling Python Face Detection Service to get faces for family ID: {familyId}.");

        var requestUri = $"/faces/family/{familyId}";
        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonContent, options);

        _logger.LogInformation($"Successfully retrieved faces for family ID: {familyId}.");
        return result ?? new List<Dictionary<string, object>>();
    }

    public async Task<Dictionary<string, string>> DeleteFaceByIdAsync(string faceId)
    {
        _logger.LogInformation($"Calling Python Face Detection Service to delete face with ID: {faceId}.");

        var requestUri = $"/faces/{faceId}";
        var response = await _httpClient.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent, options);

        _logger.LogInformation($"Successfully deleted face with ID: {faceId}.");
        return result ?? new Dictionary<string, string>();
    }

    public async Task<List<FaceSearchResultDto>> SearchFacesAsync(FaceSearchRequestDto request)
    {
        _logger.LogInformation("Calling Python Face Detection Service to search faces.");

        var requestUri = "/faces/search";
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var jsonContent = System.Text.Json.JsonSerializer.Serialize(request, options);
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUri, httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<List<FaceSearchResultDto>>(responseJson, options);

        _logger.LogInformation("Successfully searched faces.");
        return result ?? new List<FaceSearchResultDto>();
    }
}