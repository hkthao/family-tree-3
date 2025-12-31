using Amazon.S3;
using Amazon.S3.Model;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Options;
using System.Net;

namespace backend.Infrastructure.Services;

public class CloudflareR2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly CloudflareR2Settings _r2Settings;
    private readonly string _bucketName;

    public CloudflareR2FileStorageService(IOptions<CloudflareR2Settings> r2Settings)
    {
        _r2Settings = r2Settings.Value;
        _bucketName = _r2Settings.BucketName ?? throw new ArgumentNullException(nameof(_r2Settings.BucketName));

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _r2Settings.EndpointUrl,
            AuthenticationRegion = "auto", // R2 uses 'auto' region
            ForcePathStyle = true // Required for R2
        };

        _s3Client = new AmazonS3Client(
            _r2Settings.AccessKeyId,
            _r2Settings.SecretAccessKey,
            s3Config
        );
    }

    public async Task<Result<FileStorageResultDto>> UploadFileAsync(Stream fileStream, string fileName, string? folder = null, CancellationToken cancellationToken = default)
    {
        var objectKey = GetObjectKey(fileName, folder);

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = fileStream,
                ContentType = GetContentType(fileName),
                CannedACL = S3CannedACL.PublicRead // Make object publicly accessible
            };

            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                var fileUrl = GetFileUrl(objectKey);
                return Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = fileUrl, DeleteHash = null });
            }
            else
            {
                return Result<FileStorageResultDto>.Failure($"Failed to upload file to Cloudflare R2. Status: {response.HttpStatusCode}", "CloudflareR2UploadError");
            }
        }
        catch (AmazonS3Exception s3Ex)
        {
            return Result<FileStorageResultDto>.Failure($"Cloudflare R2 S3 error: {s3Ex.Message}", "CloudflareR2S3Exception");
        }
        catch (Exception ex)
        {
            return Result<FileStorageResultDto>.Failure($"An unexpected error occurred during R2 upload: {ex.Message}", "CloudflareR2UnexpectedError");
        }
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var objectKey = GetObjectKeyFromUrl(filePath);

        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File not found in Cloudflare R2: {filePath}", s3Ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve file from Cloudflare R2: {filePath}", ex);
        }
    }

    public async Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var objectKey = GetObjectKeyFromUrl(filePath);

        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            var response = await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);

            if (response.HttpStatusCode == HttpStatusCode.NoContent || response.HttpStatusCode == HttpStatusCode.OK)
            {
                return Result.Success();
            }
            else
            {
                return Result.Failure($"Failed to delete file from Cloudflare R2. Status: {response.HttpStatusCode}", "CloudflareR2DeleteError");
            }
        }
        catch (AmazonS3Exception s3Ex)
        {
            return Result.Failure($"Cloudflare R2 S3 error during deletion: {s3Ex.Message}", "CloudflareR2S3Exception");
        }
        catch (Exception ex)
        {
            return Result.Failure($"An unexpected error occurred during R2 deletion: {ex.Message}", "CloudflareR2UnexpectedError");
        }
    }

    public string GetFileUrl(string filePath)
    {
        // filePath here is actually the objectKey
        // R2's public URL format is: https://<account_id>.r2.cloudflarestorage.com/<bucket_name>/<object_key>
        return $"{_r2Settings.EndpointUrl}/{_bucketName}/{filePath}";
    }

    private string GetObjectKey(string fileName, string? folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return fileName;
        }
        return $"{folder.Trim('/')}/{fileName}";
    }

    private string GetObjectKeyFromUrl(string fileUrl)
    {
        // Example R2 URL: https://<ACCOUNT_ID>.r2.cloudflarestorage.com/<BUCKET_NAME>/folder/file.jpg
        // We need to extract 'folder/file.jpg'
        if (string.IsNullOrEmpty(fileUrl))
        {
            return string.Empty;
        }

        var uri = new Uri(fileUrl);
        // The path will be like /<bucket_name>/folder/file.jpg
        var path = uri.AbsolutePath;

        // Remove the leading slash and bucket name
        var bucketPath = $"/{_bucketName}/";
        if (path.StartsWith(bucketPath))
        {
            return path.Substring(bucketPath.Length);
        }

        // Fallback or error if URL format is unexpected
        throw new ArgumentException($"Invalid Cloudflare R2 URL format: {fileUrl}. Could not extract object key.", nameof(fileUrl));
    }


    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".pdf" => "application/pdf",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream" // Default to binary stream
        };
    }
}
