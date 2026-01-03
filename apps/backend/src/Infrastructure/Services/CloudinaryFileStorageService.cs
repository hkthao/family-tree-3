using System.Net;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _cloudinarySettings;
    private readonly ILogger<CloudinaryFileStorageService> _logger;
    private readonly string _rootFolder;

    public CloudinaryFileStorageService(IOptions<CloudinarySettings> cloudinarySettings, ILogger<CloudinaryFileStorageService> logger)
    {
        _cloudinarySettings = cloudinarySettings.Value;

        if (string.IsNullOrWhiteSpace(_cloudinarySettings.CloudName))
            throw new ArgumentNullException(nameof(_cloudinarySettings.CloudName), "Cloudinary CloudName is not configured.");
        if (string.IsNullOrWhiteSpace(_cloudinarySettings.ApiKey))
            throw new ArgumentNullException(nameof(_cloudinarySettings.ApiKey), "Cloudinary ApiKey is not configured.");
        if (string.IsNullOrWhiteSpace(_cloudinarySettings.ApiSecret))
            throw new ArgumentNullException(nameof(_cloudinarySettings.ApiSecret), "Cloudinary ApiSecret is not configured.");

        _logger = logger;
        var account = new Account(
            _cloudinarySettings.CloudName,
            _cloudinarySettings.ApiKey,
            _cloudinarySettings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
        _rootFolder = _cloudinarySettings.RootFolder.Trim('/');
    }

    public async Task<Result<FileStorageResultDto>> UploadFileAsync(Stream fileStream, string fileName, string? folder = null, CancellationToken cancellationToken = default)
    {
        var publicId = GetPublicId(fileName);
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var resourceType = GetCloudinaryResourceType(fileExtension);

        RawUploadParams uploadParams;
        object uploadResult; // Declare as object to hold various upload result types

        switch (resourceType)
        {
            case ResourceType.Image:
                uploadParams = new ImageUploadParams();
                break;
            case ResourceType.Video: // Handles both video and audio
                uploadParams = new VideoUploadParams();
                break;
            default:
                uploadParams = new RawUploadParams(); // Default to raw for unknown types
                break;
        }

        uploadParams.File = new FileDescription(fileName, fileStream);
        uploadParams.PublicId = publicId;
        uploadParams.Overwrite = true;
        uploadParams.UseFilename = true;
        uploadParams.UniqueFilename = false;
        uploadParams.Folder = Path.Combine(_cloudinarySettings.RootFolder, folder ?? "");

        try
        {
            // Dynamically call the appropriate UploadAsync overload
            if (uploadParams is ImageUploadParams imageUploadParams)
            {
                uploadResult = await _cloudinary.UploadAsync(imageUploadParams, cancellationToken);
            }
            else if (uploadParams is VideoUploadParams videoUploadParams)
            {
                uploadResult = await _cloudinary.UploadAsync(videoUploadParams, cancellationToken);
            }
            else
            {
                // Fallback or error if an unsupported uploadParams type is somehow used
                throw new InvalidOperationException("Unsupported upload parameters type.");
            }

            var baseUploadResult = uploadResult as UploadResult; // Cast to common base type

            if (baseUploadResult != null && baseUploadResult.StatusCode == HttpStatusCode.OK)
            {
                return Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = baseUploadResult.SecureUrl.AbsoluteUri, DeleteHash = baseUploadResult.PublicId });
            }
            else
            {
                var errorResult = baseUploadResult; // Use baseUploadResult if available, otherwise null
                _logger.LogError("Failed to upload file to Cloudinary. Status: {StatusCode}. Error: {Error}", errorResult?.StatusCode, errorResult?.Error?.Message);
                return Result<FileStorageResultDto>.Failure($"Failed to upload file to Cloudinary. Status: {errorResult?.StatusCode}, Error: {errorResult?.Error?.Message}", "CloudinaryUploadError");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during Cloudinary upload: {Message}", ex.Message);
            return Result<FileStorageResultDto>.Failure($"An unexpected error occurred during Cloudinary upload: {ex.Message}", "CloudinaryUnexpectedError");
        }
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Cloudinary is primarily a CDN. Getting a file as a stream generally means downloading it.
        // For direct public access, just use the URL. If a stream is absolutely needed,
        // one would typically download it from the public URL.
        // For simplicity and alignment with Cloudinary's nature, we can choose to
        // not support this method directly or implement a download from URL.
        // As per the original R2 service, it returned a stream, so we should consider
        // if this is a hard requirement for the interface.
        // For now, I'll indicate it's not directly supported in the same way R2 is.

        _logger.LogWarning("GetFileAsync is not directly supported for Cloudinary in the same way as S3-compatible storage. Use GetFileUrl to retrieve the public URL.");
        // If a download is truly needed, uncomment and adapt the following:
        // try
        // {
        //     using var httpClient = new HttpClient();
        //     var response = await httpClient.GetAsync(filePath, cancellationToken);
        //     response.EnsureSuccessStatusCode();
        //     return await response.Content.ReadAsStreamAsync();
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Failed to download file from Cloudinary URL: {FilePath}. Message: {Message}", filePath, ex.Message);
        //     throw new InvalidOperationException($"Failed to download file from Cloudinary URL: {filePath}", ex);
        // }
        throw new NotSupportedException("Retrieving file as a stream is not directly supported for Cloudinary. Please use GetFileUrl to get the public access URL.");
    }

    public async Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // filePath here is the full URL, but Cloudinary deletion requires publicId.
        // We stored PublicId in DeleteHash during upload, so we assume filePath (or a derived publicId)
        // passed to this method can be used to identify the asset.
        // Best practice is to pass the publicId directly to this method, but adhering to the interface
        // we'll try to extract it from the URL or assume filePath *is* the publicId if it's not a URL.

        var publicId = ExtractPublicIdFromUrl(filePath) ?? filePath; // Try to extract, otherwise assume filePath is publicId

        if (string.IsNullOrEmpty(publicId))
        {
            return Result.Failure("Invalid file path or public ID for deletion.", "CloudinaryDeleteError");
        }
        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        var resourceType = GetCloudinaryResourceType(fileExtension);
        var deletionParams = new DeletionParams(publicId)
        {
            ResourceType = resourceType // Use dynamic resource type
        };

        try
        {
            var destroyResult = await _cloudinary.DestroyAsync(deletionParams);

            if (destroyResult.Result == "ok")
            {
                return Result.Success();
            }
            else
            {
                _logger.LogError("Failed to delete file from Cloudinary. PublicId: {PublicId}. Result: {Result}. Error: {Error}", publicId, destroyResult.Result, destroyResult.Error?.Message);
                return Result.Failure($"Failed to delete file from Cloudinary. PublicId: {publicId}. Result: {destroyResult.Result}, Error: {destroyResult.Error?.Message}", "CloudinaryDeleteError");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during Cloudinary deletion: {Message}", ex.Message);
            return Result.Failure($"An unexpected error occurred during Cloudinary deletion: {ex.Message}", "CloudinaryUnexpectedError");
        }
    }

    public string GetFileUrl(string filePath)
    {
        // For Cloudinary, filePath would be the PublicId or the existing full URL.
        // If it's a PublicId, we construct the URL. If it's already a URL, we return it.
        if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return filePath; // Already a full URL
        }

        // Assume filePath is a publicId
        var transformation = new Transformation().FetchFormat("auto"); // Example: automatically select best format
        return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(filePath);
    }

    private string GetPublicId(string fileName)
    {
        // Remove extension from filename to use as public ID
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return $"{_rootFolder}_{nameWithoutExtension}";
    }

    private CloudinaryDotNet.Actions.ResourceType GetCloudinaryResourceType(string fileExtension)
    {
        return fileExtension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => ResourceType.Image,
            ".mp4" or ".avi" or ".mov" or ".webm" => ResourceType.Video,
            ".mp3" or ".wav" or ".ogg" or ".flac" => ResourceType.Video, // Cloudinary often treats audio as video resource type
            ".pdf" => ResourceType.Raw, // PDFs are treated as raw files in Cloudinary
            _ => ResourceType.Raw // Default to raw for unknown types
        };
    }

    private string? ExtractPublicIdFromUrl(string fileUrl)
    {
        // Cloudinary URLs often look like:
        // https://res.cloudinary.com/<cloud_name>/image/upload/<version>/<public_id>.<extension>
        // We need to extract <public_id>
        try
        {
            var uri = new Uri(fileUrl);
            var pathSegments = uri.Segments;

            // Find the "upload" segment, public ID should follow it.
            // Or look for segments that don't look like versions or directories.
            // This is a simplification; a more robust solution might use regex or Cloudinary's own URL parsing.

            // Common pattern: /upload/v<version_number>/<public_id_with_folders>.<extension>
            // We want everything after /upload/ and before the last dot (extension).

            int uploadIndex = Array.IndexOf(pathSegments, "upload/");
            if (uploadIndex > -1 && pathSegments.Length > uploadIndex + 1)
            {
                // Find the first segment after "upload/" that contains 'v' followed by digits (version)
                // If there's a version, public_id comes after it.
                // Otherwise, public_id comes directly after 'upload/'.

                int publicIdStartIndex = uploadIndex + 1;
                if (publicIdStartIndex < pathSegments.Length && pathSegments[publicIdStartIndex].StartsWith("v") && pathSegments[publicIdStartIndex].Trim('/').All(c => char.IsDigit(c) || c == 'v'))
                {
                    // This is likely a version segment, so public_id is after it.
                    publicIdStartIndex++;
                }

                if (publicIdStartIndex < pathSegments.Length)
                {
                    var publicIdWithExtension = string.Join("", pathSegments.Skip(publicIdStartIndex)).TrimEnd('/');
                    var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                    if (lastDotIndex > -1)
                    {
                        return publicIdWithExtension.Substring(0, lastDotIndex);
                    }
                    return publicIdWithExtension; // No extension found, return as is
                }
            }
            // Fallback for cases where publicId might be directly after cloud_name for root assets
            // Example: https://res.cloudinary.com/<cloud_name>/image/upload/<public_id>
            // This case is typically handled by the /upload/ path.

            // If the URL doesn't conform to the expected pattern for public_id extraction, return null.
            return null;
        }
        catch (UriFormatException ex)
        {
            _logger.LogWarning(ex, "Invalid URL format during public ID extraction: {FileUrl}", fileUrl);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting public ID from URL: {FileUrl}", fileUrl);
            return null;
        }
    }


}
