using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs; // Moved DTOs
namespace backend.Application.Files.UploadFile;

public class UploadFileCommandHandler(
    IFileStorageService fileStorageService
) : IRequestHandler<UploadFileCommand, Result<ImageUploadResponseDto>>
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    public async Task<Result<ImageUploadResponseDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        using var fileStream = new MemoryStream(request.ImageData);
        var uploadResult = await _fileStorageService.UploadFileAsync(
            fileStream,
            request.FileName,
            request.Folder,
            cancellationToken
        );

        if (!uploadResult.IsSuccess)
        {
            return Result<ImageUploadResponseDto>.Failure(uploadResult.Error ?? ErrorMessages.FileUploadFailed, uploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (string.IsNullOrEmpty(uploadResult.Value))
        {
            return Result<ImageUploadResponseDto>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
        }

        // The IFileStorageService returns the URL directly.
        // We construct a response similar to the original n8n webhook response.
        var responseDto = new ImageUploadResponseDto
        {
            Url = uploadResult.Value,
            DisplayUrl = uploadResult.Value, // Assuming display URL is the same for direct access
            Filename = request.FileName, // Use Filename property
            ContentType = request.ContentType,
            Extension = Path.GetExtension(request.FileName)
        };

        return Result<ImageUploadResponseDto>.Success(responseDto);
    }
}
