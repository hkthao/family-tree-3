using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Infrastructure.Files;

public class CloudinaryFileStorage : IFileStorageService
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
