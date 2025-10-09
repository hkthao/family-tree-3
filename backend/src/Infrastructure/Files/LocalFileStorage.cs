using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Files;

public class LocalFileStorage : IFileStorageService
{
    private readonly StorageSettings _storageSettings;
    private readonly IWebHostEnvironment _env; // To get wwwroot path

    public LocalFileStorage(IOptions<StorageSettings> storageSettings, IWebHostEnvironment env)
    {
        _storageSettings = storageSettings.Value;
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

            // Return a URL relative to the web root
            var relativePath = Path.Combine(_storageSettings.LocalStoragePath, fileName).Replace("\\", "/");
            return Result<string>.Success($"/{relativePath}");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Local file upload failed: {ex.Message}", "LocalFileStorage");
        }
    }
}
