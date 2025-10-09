using backend.Application.Common.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Defines the interface for file storage operations.
/// </summary>
public interface IFileStorageService
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
}
