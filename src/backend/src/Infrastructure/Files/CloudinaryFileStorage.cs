using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Infrastructure.Files;

public class CloudinaryFileStorage : IFileStorage
{
    private readonly Cloudinary _cloudinary;
    private readonly IConfigProvider _configProvider;

    public CloudinaryFileStorage(IConfigProvider configProvider)
    {
        _configProvider = configProvider;
        var storageSettings = _configProvider.GetSection<StorageSettings>();
        var account = new Account(
            storageSettings.Cloudinary.CloudName,
            storageSettings.Cloudinary.ApiKey,
            storageSettings.Cloudinary.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }
    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        _ = _configProvider.GetSection<StorageSettings>();
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

            return uploadResult.Error != null
                ? Result<string>.Failure(uploadResult.Error.Message, "Cloudinary")
                : Result<string>.Success(uploadResult.SecureUrl.ToString());
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

            return deletionResult.Error != null ? Result.Failure(deletionResult.Error.Message, "Cloudinary") : Result.Success();
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
        else
        {
            return contentType.StartsWith("video") ? ResourceType.Video : ResourceType.Raw;
        }
    }
}
