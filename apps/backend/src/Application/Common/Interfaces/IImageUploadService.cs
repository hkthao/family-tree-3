using backend.Application.Common.Models;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace backend.Application.Common.Interfaces;

public interface IImageUploadService
{
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string? fileName = null, int? expiration = null);
}
