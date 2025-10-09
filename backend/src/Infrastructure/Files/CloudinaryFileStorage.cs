using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Infrastructure.Files;

public class CloudinaryFileStorage : IFileStorage
{
    private readonly Cloudinary _cloudinary;
    private readonly IStorageSettings _storageSettings;

    public CloudinaryFileStorage(IStorageSettings storageSettings)
    {
        _storageSettings = storageSettings;
        var account = new Account(
            _storageSettings.Cloudinary.CloudName,
            _storageSettings.Cloudinary.ApiKey,
            _storageSettings.Cloudinary.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var resourceType = GetResourceType(contentType);

            RawUploadResult uploadResult;

            switch (resourceType)
            {
                case ResourceType.Image:
                    var imageParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileName, fileStream),
                        Folder = "family-tree-uploads/images",
                        PublicId = Path.GetFileNameWithoutExtension(fileName),
                        Overwrite = false
                    };
                    uploadResult = await _cloudinary.UploadAsync(imageParams, cancellationToken);
                    break;

                case ResourceType.Video:
                    var videoParams = new VideoUploadParams
                    {
                        File = new FileDescription(fileName, fileStream),
                        Folder = "family-tree-uploads/videos",
                        PublicId = Path.GetFileNameWithoutExtension(fileName),
                        Overwrite = false
                    };
                    uploadResult = await _cloudinary.UploadAsync(videoParams, cancellationToken);
                    break;

                default:
                    var rawParams = new RawUploadParams
                    {
                        File = new FileDescription(fileName, fileStream),
                        Folder = "family-tree-uploads/raw",
                        PublicId = Path.GetFileNameWithoutExtension(fileName),
                        Overwrite = false
                    };
                    uploadResult = await _cloudinary.UploadAsync(rawParams, "", cancellationToken);
                    break;
            }

            if (uploadResult.Error != null)
                return Result<string>.Failure(uploadResult.Error.Message, "Cloudinary");

            return Result<string>.Success(uploadResult.SecureUrl.ToString());
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Cloudinary upload failed: {ex.Message}", "Cloudinary");
        }
    }

    public async Task<Result> DeleteFileAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var publicId = GetPublicIdFromUrl(url);
            if (string.IsNullOrEmpty(publicId))
            {
                return Result.Failure("Could not extract Public ID from URL.", "Cloudinary");
            }

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Error != null)
            {
                return Result.Failure(deletionResult.Error.Message, "Cloudinary");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Cloudinary deletion failed: {ex.Message}", "Cloudinary");
        }
    }

    private string GetPublicIdFromUrl(string url)
    {
        // Example: https://res.cloudinary.com/cloud_name/image/upload/v12345/folder/public_id.jpg
        // We need to extract 'folder/public_id'
        var uri = new Uri(url);
        var segments = uri.Segments;
        if (segments.Length < 3) return null!;

        var publicIdWithExtension = segments[^1]; // last segment
        var folder = segments[^2].TrimEnd('/'); // second to last segment

        var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);

        if (folder == "upload") return publicId; // If no specific folder was used

        return $"{folder}/{publicId}";
    }

    private ResourceType GetResourceType(string contentType)
    {
        if (contentType.StartsWith("image"))
        {
            return ResourceType.Image;
        }
        else if (contentType.StartsWith("video"))
        {
            return ResourceType.Video;
        }
        else
        {
            return ResourceType.Raw;
        }
    }
}
