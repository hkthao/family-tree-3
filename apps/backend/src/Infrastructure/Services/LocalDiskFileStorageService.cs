using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai IFileStorageService để xử lý các hoạt động trên hệ thống file cục bộ.
/// </summary>
public class LocalDiskFileStorageService : IFileStorageService
{
    private readonly string _localStoragePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalDiskFileStorageService> _logger;

    public LocalDiskFileStorageService(IOptions<FileStorageSettings> fileStorageSettings, ILogger<LocalDiskFileStorageService> logger)
    {
        // Use FileStorageSettings.Local.LocalStoragePath for consistency, assuming it's correctly configured
        _localStoragePath = Path.Combine(Directory.GetCurrentDirectory(), fileStorageSettings.Value.Local.LocalStoragePath);
        _baseUrl = fileStorageSettings.Value.Local.BaseUrl;
        _logger = logger;

        // Ensure the local storage directory exists
        if (!Directory.Exists(_localStoragePath))
        {
            _logger.LogInformation("Creating local storage directory: {Path}", _localStoragePath);
            Directory.CreateDirectory(_localStoragePath);
        }
    }

    /// <summary>
    /// Lưu một file vào bộ nhớ cục bộ.
    /// </summary>
    /// <param name="fileStream">Stream của file cần lưu.</param>
    /// <param name="fileName">Tên file.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Đường dẫn đầy đủ đến file đã lưu cục bộ.</returns>
    public async Task<Result<FileStorageResultDto>> SaveFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default)
    {
        // Generate a unique file name to avoid conflicts, preserving original extension
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var finalPath = Path.Combine(_localStoragePath, uniqueFileName);

        // Ensure target folder exists if 'folder' is specified and needs to create subdirectories within _localStoragePath
        string targetDirectory = _localStoragePath;
        if (!string.IsNullOrEmpty(folder))
        {
            // Note: For local disk storage, the 'folder' parameter typically would be part of the relative path
            // from _localStoragePath. For simplicity here, we append it directly to the base path.
            // Adjust if a more complex sub-directory structure is needed.
            targetDirectory = Path.Combine(_localStoragePath, folder);
            if (!Directory.Exists(targetDirectory))
            {
                _logger.LogInformation("Creating sub-directory for local storage: {Path}", targetDirectory);
                Directory.CreateDirectory(targetDirectory);
            }
            finalPath = Path.Combine(targetDirectory, uniqueFileName);
        }

        using (var file = new FileStream(finalPath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(file, cancellationToken);
        }

        _logger.LogInformation("File saved locally: {FileName} at {FilePath}", fileName, finalPath);

        // Return the local file path. It will be the "FilePath" in FamilyMedia and sent via message bus.
        return Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = finalPath, DeleteHash = null });
    }

    /// <summary>
    /// Đọc một file từ bộ nhớ cục bộ.
    /// </summary>
    /// <param name="filePath">Đường dẫn đầy đủ đến file cục bộ.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Stream của file.</returns>
    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found at local path: {FilePath}", filePath);
            throw new FileNotFoundException($"File not found at {filePath}");
        }

        return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    /// <summary>
    /// Xóa một file khỏi bộ nhớ cục bộ.
    /// </summary>
    /// <param name="filePath">Đường dẫn đầy đủ đến file cục bộ.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    public Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted local file: {FilePath}", filePath);
                return Task.FromResult(Result.Success());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting local file {FilePath}: {Message}", filePath, ex.Message);
                return Task.FromResult(Result.Failure($"Error deleting local file: {ex.Message}"));
            }
        }
        _logger.LogWarning("Attempted to delete non-existent local file: {FilePath}", filePath);
        return Task.FromResult(Result.Success()); // Treat deleting non-existent file as success
    }

    /// <summary>
    /// Lấy URL truy cập công khai đến file.
    /// Đối với lưu trữ cục bộ, đây sẽ là đường dẫn cục bộ hoặc URL có thể truy cập qua web server tĩnh.
    /// </summary>
    /// <param name="filePath">Đường dẫn đến file.</param>
    /// <returns>URL truy cập công khai.</returns>
    public string GetFileUrl(string filePath)
    {
        // For local files, if they are served via a static file server,
        // filePath is the full disk path, we need to convert it to a URL.
        // Assuming _baseUrl is configured to point to the static files served from _localStoragePath.
        if (filePath.StartsWith(_localStoragePath))
        {
            var relativePath = Path.GetRelativePath(_localStoragePath, filePath).Replace('\\', '/');
            return $"{_baseUrl}/{relativePath}";
        }
        return filePath; // If it's not a local path, return as is (might be a cloud URL already)
    }
}
