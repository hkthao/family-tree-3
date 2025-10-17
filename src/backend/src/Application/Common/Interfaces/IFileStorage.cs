using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Defines the interface for file storage operations.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Uploads a file to the configured storage provider.
    /// </summary>
    /// <param name="fileStream">The stream of the file to upload.</param>
    /// <param name="fileName">The original name of the file.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the URL of the uploaded file on success, or an error on failure.</returns>
    Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a file from the configured storage provider.
    /// </summary>
    /// <param name="url">The URL of the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result> DeleteFileAsync(string url, CancellationToken cancellationToken);
}
