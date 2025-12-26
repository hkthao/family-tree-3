using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FamilyTree.Infrastructure.Services;

public class ImgbbImageUploadService : IImageUploadService
{
    private readonly HttpClient _httpClient;
    private readonly ImgbbSettings _imgbbSettings;

    public ImgbbImageUploadService(HttpClient httpClient, IOptions<ImgbbSettings> imgbbSettings)
    {
        _httpClient = httpClient;
        _imgbbSettings = imgbbSettings.Value;

        if (string.IsNullOrEmpty(_imgbbSettings.ApiKey))
        {
            throw new ArgumentNullException(nameof(_imgbbSettings.ApiKey), "Imgbb API Key is not configured.");
        }

        _httpClient.BaseAddress = new Uri(_imgbbSettings.BaseUrl);
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string? fileName = null, int? expiration = null)
    {
        using var content = new MultipartFormDataContent();

        // Convert IFormFile to byte array and then to Base64
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var base64Image = Convert.ToBase64String(fileBytes);

        content.Add(new StringContent(_imgbbSettings.ApiKey), "key");
        content.Add(new StringContent(base64Image), "image");

        if (!string.IsNullOrEmpty(fileName))
        {
            content.Add(new StringContent(fileName), "name");
        }

        if (expiration.HasValue)
        {
            content.Add(new StringContent(expiration.Value.ToString()), "expiration");
        }

        var response = await _httpClient.PostAsync("upload", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var imgbbResponse = JsonSerializer.Deserialize<ImgbbUploadResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (imgbbResponse?.Success != true || imgbbResponse?.Data == null)
        {
            throw new Exception($"Imgbb upload failed: {imgbbResponse?.Error?.Message ?? responseString}");
        }

        // Parse Size from string to long
        long.TryParse(imgbbResponse.Data.Size, out long parsedSize);
        int.TryParse(imgbbResponse.Data.Image.Width, out int parsedWidth);
        int.TryParse(imgbbResponse.Data.Image.Height, out int parsedHeight);

        return new ImageUploadResultDto
        {
            Id = imgbbResponse.Data.Id,
            Title = imgbbResponse.Data.Title,
            Url = imgbbResponse.Data.Url,
            DeleteUrl = imgbbResponse.Data.DeleteUrl,
            MimeType = imgbbResponse.Data.Mime, // Mime type is directly available
            Size = parsedSize,
            Width = parsedWidth,
            Height = parsedHeight
        };
    }

    // Helper classes to deserialize imgbb API response
    private class ImgbbUploadResponse
    {
        public ImgbbImageData? Data { get; set; } // Made nullable
        public bool Success { get; set; }
        public int Status { get; set; }
        public ImgbbError? Error { get; set; } // Added for error handling
    }

    private class ImgbbError
    {
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    private class ImgbbImageData
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string UrlViewer { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty; // The actual image URL
        public string DisplayUrl { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty; // imgbb returns size as string
        public string Mime { get; set; } = string.Empty; // imgbb returns mime as string
        public string Extension { get; set; } = string.Empty;
        public string DeleteUrl { get; set; } = string.Empty;
        public ImgbbImageDetails Image { get; set; } = new();
        public ImgbbImageDetails Thumb { get; set; } = new();
        public ImgbbImageDetails Medium { get; set; } = new();
    }

    private class ImgbbImageDetails
    {
        public string Filename { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Mime { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Width { get; set; } = string.Empty; // Width as string
        public string Height { get; set; } = string.Empty; // Height as string
    }
}
