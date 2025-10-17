using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Infrastructure.Files;

public class LocalFileStorage : IFileStorage
{
    private readonly StorageSettings _storageSettings;

    public LocalFileStorage(StorageSettings storageSettings)
    {
        _storageSettings = storageSettings;
    }

    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var uploadPath = Path.Combine(_storageSettings.Local.LocalStoragePath);
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
            var absoluteUrl = $"{_storageSettings.Local.BaseUrl}/api/upload/preview/{fileName}"; return Result<string>.Success(absoluteUrl);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Local file upload failed: {ex.Message}", "LocalFileStorage");
        }
    }

    public Task<Result> DeleteFileAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            // Extract file name from the URL
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);

            var filePath = Path.Combine(_storageSettings.Local.LocalStoragePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(Result.Success());
            }
            return Task.FromResult(Result.Failure("File not found locally.", "LocalFileStorage"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure($"Local file deletion failed: {ex.Message}", "LocalFileStorage"));
        }
    }
}
