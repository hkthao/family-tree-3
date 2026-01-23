using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs; // Moved DTOs

namespace backend.Application.Files.Commands.UploadFileFromUrl;

public class UploadFileFromUrlCommandHandler : IRequestHandler<UploadFileFromUrlCommand, Result<ImageUploadResponseDto>>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UploadFileFromUrlCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<ImageUploadResponseDto>> Handle(UploadFileFromUrlCommand request, CancellationToken cancellationToken)
    {
        // 1. Download file from URL
        var httpClient = _httpClientFactory.CreateClient();
        byte[] imageData;
        string contentType;

        try
        {
            var response = await httpClient.GetAsync(request.FileUrl, cancellationToken);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status code is not 2xx.

            imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        }
        catch (HttpRequestException ex)
        {
            return Result<ImageUploadResponseDto>.Failure($"Failed to download file from URL: {ex.Message}", ErrorSources.ExternalServiceError);
        }
        catch (Exception ex)
        {
            return Result<ImageUploadResponseDto>.Failure($"An unexpected error occurred while downloading the file: {ex.Message}", ErrorSources.ExternalServiceError);
        }

        // Placeholder for removed n8n service call.
        // If file upload functionality is needed, it should be reimplemented here.
        return Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto
        {
            Name = request.FileName, // Assuming request.FileName maps to Name
            Filename = request.FileName, // Assuming request.FileName maps to Filename
            Url = $"mock-url-for-{request.FileName}",
            ContentType = contentType
        });
    }
}
