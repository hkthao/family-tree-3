using System.Net.Http.Headers;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class ImgurFileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly IApplicationDbContext _context;

    public ImgurFileStorageService(HttpClient httpClient, IOptions<ImgurSettings> imgurSettings, IApplicationDbContext context)
    {
        _httpClient = httpClient;
        _clientId = imgurSettings.Value.ClientId ?? throw new ArgumentNullException(nameof(imgurSettings.Value.ClientId));
        _context = context;

        _httpClient.BaseAddress = new Uri("https://api.imgur.com/3/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _clientId);
    }

    public async Task<Result<FileStorageResultDto>> UploadFileAsync(Stream fileStream, string fileName, string? folder = null, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var imageData = memoryStream.ToArray();

        using var content = new ByteArrayContent(imageData);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var request = new HttpRequestMessage(HttpMethod.Post, "image")
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<FileStorageResultDto>.Failure($"Failed to upload image to Imgur: {response.ReasonPhrase}. Details: {errorResponse}", "ImgurUploadError");
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var imgurResponse = JsonSerializer.Deserialize<ImgurUploadResponse>(responseString);

        if (imgurResponse?.Data?.Link == null || imgurResponse.Data.Deletehash == null)
        {
            return Result<FileStorageResultDto>.Failure("Failed to upload image to Imgur or retrieve link/deletehash.", "ImgurUploadError");
        }

        return Result<FileStorageResultDto>.Success(new FileStorageResultDto
        {
            FileUrl = imgurResponse.Data.Link,
            DeleteHash = imgurResponse.Data.Deletehash
        });
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Downloading files directly via ImgurFileStorageService is not implemented. Use the provided URL to access the file.");
    }

    public async Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // To delete, we need the deletehash.
        // The filePath is the Imgur URL. We need to find the FamilyMedia entity associated with this URL
        // and retrieve its DeleteHash.
        var familyMedia = await _context.FamilyMedia.FirstOrDefaultAsync(fm => fm.FilePath == filePath, cancellationToken);

        if (familyMedia == null)
        {
            return Result.Failure("FamilyMedia entity not found for the given file path.", "FileNotFound");
        }

        if (string.IsNullOrEmpty(familyMedia.DeleteHash))
        {
            return Result.Failure("Delete hash not found for this file. Cannot delete from Imgur.", "DeleteHashMissing");
        }

        var request = new HttpRequestMessage(HttpMethod.Delete, $"image/{familyMedia.DeleteHash}");
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure($"Failed to delete image from Imgur: {response.ReasonPhrase}. Details: {errorResponse}", "ImgurDeleteError");
        }

        // Optionally, remove the FamilyMedia entry from the database or clear DeleteHash/FilePath
        // This is typically handled by the application layer after successful deletion from storage.
        // For now, just indicate success.
        return Result.Success();
    }

    public string GetFileUrl(string filePath)
    {
        return filePath;
    }

    // Helper classes for JSON deserialization
    private class ImgurUploadResponse
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public ImgurData? Data { get; set; }
    }

    private class ImgurData
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Datetime { get; set; }
        public string? Type { get; set; }
        public bool Animated { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get; set; }
        public int Views { get; set; }
        public int Bandwidth { get; set; }
        public string? Vote { get; set; }
        public bool Favorite { get; set; }
        public bool Nsfw { get; set; }
        public string? Section { get; set; }
        public string? AccountUrl { get; set; }
        public int AccountId { get; set; }
        public bool IsAd { get; set; }
        public bool InMostViral { get; set; }
        public bool HasSound { get; set; }
        public string[]? Tags { get; set; }
        public int AdType { get; set; }
        public string? AdUrl { get; set; }
        public bool InGallery { get; set; }
        public string? Deletehash { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; }
    }
}
