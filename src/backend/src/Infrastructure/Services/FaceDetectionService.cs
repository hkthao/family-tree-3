namespace FamilyTree.Infrastructure.Services;

using System.Net.Http.Json;
using backend.Application.Faces.Common.Interfaces;
using backend.Application.Faces.Commands;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

public class FaceDetectionService : IFaceDetectionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FaceDetectionService> _logger;

    public FaceDetectionService(HttpClient httpClient, ILogger<FaceDetectionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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