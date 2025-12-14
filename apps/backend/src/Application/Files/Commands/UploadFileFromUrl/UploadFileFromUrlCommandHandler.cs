using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs; // Moved DTOs

namespace backend.Application.Files.Commands.UploadFileFromUrl;

public class UploadFileFromUrlCommandHandler : IRequestHandler<UploadFileFromUrlCommand, Result<ImageUploadResponseDto>>
{
    private readonly IN8nService _n8nService;
    private readonly IHttpClientFactory _httpClientFactory;

    public UploadFileFromUrlCommandHandler(IN8nService n8nService, IHttpClientFactory httpClientFactory)
    {
        _n8nService = n8nService;
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

        // 2. Construct ImageUploadWebhookDto
        var imageUploadDto = new ImageUploadWebhookDto
        {
            ImageData = imageData,
            FileName = request.FileName,
            Folder = request.Folder,
            ContentType = contentType // Pass content type
        };
        // 3. Call n8n Image Upload Webhook
        var n8nUploadResult = await _n8nService.CallImageUploadWebhookAsync(imageUploadDto, cancellationToken);
        if (!n8nUploadResult.IsSuccess)
        {
            return Result<ImageUploadResponseDto>.Failure(n8nUploadResult.Error ?? ErrorMessages.FileUploadFailed, n8nUploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (n8nUploadResult.Value == null)
        {
            return Result<ImageUploadResponseDto>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
        }

        var uploadedImageUrl = n8nUploadResult.Value.Url;

        if (string.IsNullOrEmpty(uploadedImageUrl))
        {
            return Result<ImageUploadResponseDto>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
        }

        return Result<ImageUploadResponseDto>.Success(n8nUploadResult.Value);
    }
}
