using backend.Application.Common.Models;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace backend.Application.Images.Commands.UploadImage;

public record UploadImageCommand : IRequest<ImageUploadResultDto>
{
    public IFormFile? File { get; init; }
    public string? FileName { get; init; }
    public int? Expiration { get; init; } // Optional: Image expiration in seconds (60-15552000)
}
