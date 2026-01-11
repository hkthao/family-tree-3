using System.Net.Mime; // For MediaTypeNames.Application.Octet
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs; // Moved DTOs

namespace backend.Infrastructure.Services;

public class N8nFileStorageService : IFileStorageService
{
    private readonly IN8nService _n8nService;

    public N8nFileStorageService(IN8nService n8nService)
    {
        _n8nService = n8nService;
    }

    public async Task<Result<FileStorageResultDto>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default)
    {
        // Convert Stream to byte array
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var imageData = memoryStream.ToArray();

        var imageUploadDto = new ImageUploadWebhookDto
        {
            ImageData = imageData,
            FileName = fileName,
            Folder = folder ?? "general", // Default folder if not specified
            ContentType = contentType // Use the provided contentType
        };

        var n8nUploadResult = await _n8nService.CallImageUploadWebhookAsync(imageUploadDto, cancellationToken);

        if (!n8nUploadResult.IsSuccess)
        {
            return Result<FileStorageResultDto>.Failure(n8nUploadResult.Error ?? "File upload failed via n8n.", n8nUploadResult.ErrorSource ?? "ExternalServiceError");
        }

        if (n8nUploadResult.Value == null || string.IsNullOrEmpty(n8nUploadResult.Value.Url))
        {
            return Result<FileStorageResultDto>.Failure("N8n webhook did not return a valid file URL.", "ExternalServiceError");
        }

        return Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = n8nUploadResult.Value.Url, DeleteHash = null });
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // For cloud storage via n8n, we typically get a direct URL.
        // Downloading from a URL is not a direct responsibility of IFileStorageService implementation
        // if the service primarily handles uploads and provides URLs.
        // A dedicated download service or direct HTTP client would handle this.
        throw new NotImplementedException("Downloading files directly via N8nFileStorageService is not implemented. Use the provided URL to access the file.");
    }

    public Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // N8n webhook typically only handles uploads. Deletion would require a separate n8n workflow
        // or direct access to the underlying cloud storage API.
        return Task.FromResult(Result.Failure("File deletion via N8nFileStorageService is not implemented.", "NotImplemented"));
    }

    public string GetFileUrl(string filePath)
    {
        // If n8n returns a direct URL on upload, then filePath IS the URL.
        // No further processing is needed here.
        return filePath;
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".pdf" => "application/pdf",
            _ => MediaTypeNames.Application.Octet // Default to binary stream
        };
    }
}
