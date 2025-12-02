using backend.Application.AI.DTOs; // NEW
using backend.Application.Common.Models;

namespace backend.Application.Files.UploadFile;

/// <summary>
/// Command to upload a file to external storage via n8n webhook.
/// </summary>
public record UploadFileCommand : IRequest<Result<ImageUploadResponseDto>>
{
    /// <summary>
    /// The image data in bytes.
    /// </summary>
    public byte[] ImageData { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// The original name of the file (e.g., "myimage.jpg").
    /// </summary>
    public string FileName { get; init; } = null!;

    /// <summary>
    /// The folder in the cloud storage to upload to (e.g., "family-tree-memories").
    /// </summary>
    public string Folder { get; init; } = string.Empty; // Default folder
    public string ContentType { get; init; } = string.Empty;
}
