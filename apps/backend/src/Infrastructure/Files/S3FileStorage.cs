using Amazon.S3;
using Amazon.S3.Model;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Configuration;

namespace backend.Infrastructure.Files;

public class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;

    public S3FileStorage(IConfiguration configuration)
    {
        _configuration = configuration;
        var storageSettings = _configuration.GetSection(nameof(StorageSettings)).Get<StorageSettings>() ?? new StorageSettings();
        _s3Client = new AmazonS3Client(
            storageSettings.S3.AccessKey,
            storageSettings.S3.SecretKey,
            Amazon.RegionEndpoint.GetBySystemName(storageSettings.S3.Region));
    }
    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var storageSettings = _configuration.GetSection(nameof(StorageSettings)).Get<StorageSettings>() ?? new StorageSettings();
        try
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = storageSettings.S3.BucketName,
                Key = fileName,
                ContentType = contentType,
                InputStream = fileStream,
                CannedACL = S3CannedACL.PublicRead // Make the uploaded file publicly accessible
            };

            await _s3Client.PutObjectAsync(putObjectRequest, cancellationToken);

            // Construct the public URL for the uploaded file
            var fileUrl = $"https://{storageSettings.S3.BucketName}.s3.{storageSettings.S3.Region}.amazonaws.com/{fileName}";
            return Result<string>.Success(fileUrl);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"S3 upload failed: {ex.Message}", "S3FileStorage");
        }
    }

    public async Task<Result> DeleteFileAsync(string url, CancellationToken cancellationToken)
    {
        var storageSettings = _configuration.GetSection(nameof(StorageSettings)).Get<StorageSettings>() ?? new StorageSettings();
        try
        {
            // Extract file name from the URL
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = storageSettings.S3.BucketName,
                Key = fileName
            };
            await _s3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"S3 deletion failed: {ex.Message}", "S3FileStorage");
        }
    }
}
