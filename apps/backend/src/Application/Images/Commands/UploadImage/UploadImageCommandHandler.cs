using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.AspNetCore.Http; // For BadHttpRequestException

namespace backend.Application.Images.Commands.UploadImage;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, ImageUploadResultDto>
{
    private readonly IImageUploadService _imageUploadService;

    public UploadImageCommandHandler(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    public async Task<ImageUploadResultDto> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        // Basic validation for file size (e.g., max 32MB for imgBB)
        // This could be moved to a FluentValidation pipeline behavior
        if (request.File == null || request.File.Length == 0)
        {
            throw new BadHttpRequestException("File is empty.");
        }

        // 32 MB limit for imgBB
        if (request.File.Length > 32 * 1024 * 1024)
        {
            throw new BadHttpRequestException("File size exceeds 32MB limit.");
        }

        return await _imageUploadService.UploadImageAsync(
            request.File,
            request.FileName,
            request.Expiration);
    }
}
