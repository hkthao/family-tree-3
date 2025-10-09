using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.AspNetCore.Hosting;

namespace backend.Infrastructure.Files;

public class LocalFileStorage : IFileStorageService
{
    private readonly IStorageSettings _storageSettings;
    private readonly IWebHostEnvironment _env; // To get wwwroot path

    public LocalFileStorage(IStorageSettings storageSettings, IWebHostEnvironment env)
    {
        _storageSettings = storageSettings;
        _env = env;
    }

    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var uploadPath = Path.Combine(_env.WebRootPath, _storageSettings.LocalStoragePath);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            await using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream, cancellationToken);
            }

            // Construct the API preview URL
            var absoluteUrl = $"{_storageSettings.BaseUrl}/api/upload/preview/{fileName}";

            return Result<string>.Success(absoluteUrl);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Local file upload failed: {ex.Message}", "LocalFileStorage");
        }
    }
}
