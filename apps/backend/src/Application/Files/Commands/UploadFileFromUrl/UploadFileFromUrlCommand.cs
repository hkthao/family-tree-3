using backend.Application.Common.Models;
using backend.Application.AI.DTOs;

namespace backend.Application.Files.Commands.UploadFileFromUrl;

/// <summary>
/// Command to upload a file from a URL to external storage via n8n webhook.
/// </summary>
public record UploadFileFromUrlCommand : IRequest<Result<ImageUploadResponseDto>>
{
    /// <summary>
    /// The URL of the file to upload.
    /// </summary>
    public string FileUrl { get; init; } = null!;

    /// <summary>
    /// The desired name of the file (e.g., "myimage.jpg").
    /// </summary>
    public string FileName { get; init; } = null!;

    /// <summary>
    /// The cloud storage service to use (e.g., "imgbb").
    /// </summary>
    public string Cloud { get; init; } = "imgbb"; // Default to imgbb

    /// <summary>
    /// The folder in the cloud storage to upload to (e.g., "family-tree-memories").
    /// </summary>
    public string Folder { get; init; } = "family-tree-memories"; // Default folder
}
