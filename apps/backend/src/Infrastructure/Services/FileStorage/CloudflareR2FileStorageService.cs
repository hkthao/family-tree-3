using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging; // Added for ILogger
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class CloudflareR2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly CloudflareR2Settings _r2Settings;
    private readonly string _bucketName;
    private readonly ILogger<CloudflareR2FileStorageService> _logger; // Injected ILogger

    public CloudflareR2FileStorageService(IOptions<CloudflareR2Settings> r2Settings, ILogger<CloudflareR2FileStorageService> logger)
    {
        _r2Settings = r2Settings.Value;

        if (string.IsNullOrWhiteSpace(_r2Settings.AccountId))
            throw new ArgumentNullException(nameof(_r2Settings.AccountId), "Cloudflare R2 AccountId is not configured.");
        if (string.IsNullOrWhiteSpace(_r2Settings.AccessKeyId))
            throw new ArgumentNullException(nameof(_r2Settings.AccessKeyId), "Cloudflare R2 AccessKeyId is not configured.");
        if (string.IsNullOrWhiteSpace(_r2Settings.SecretAccessKey))
            throw new ArgumentNullException(nameof(_r2Settings.SecretAccessKey), "Cloudflare R2 SecretAccessKey is not configured.");
        if (string.IsNullOrWhiteSpace(_r2Settings.BucketName))
            throw new ArgumentNullException(nameof(_r2Settings.BucketName), "Cloudflare R2 BucketName is not configured.");
        if (string.IsNullOrWhiteSpace(_r2Settings.EndpointUrl))
            throw new ArgumentNullException(nameof(_r2Settings.EndpointUrl), "Cloudflare R2 EndpointUrl is not configured.");

        _bucketName = _r2Settings.BucketName;
        _logger = logger;

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _r2Settings.EndpointUrl,
            // AuthenticationRegion = "", // Explicitly empty for R2
            //  ForcePathStyle = true // Required for R2
        };

        _s3Client = new AmazonS3Client(
            _r2Settings.AccessKeyId,
            _r2Settings.SecretAccessKey,
            s3Config
        );
    }

    public async Task<Result<FileStorageResultDto>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default)
    {
        var objectKey = GetObjectKey(fileName, folder);

        try
        {
            // Read the file stream into a MemoryStream to get the content length.
            // This is necessary because Cloudflare R2 does not support STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER
            // which the AWS SDK might use if the input stream length is not known.
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0; // Reset position to the beginning for upload

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = memoryStream,
                ContentType = contentType, // Use the provided contentType
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
                // CannedACL = S3CannedACL.PublicRead, // Make object publicly accessible
                // DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                var fileUrl = GetFileUrl(objectKey);
                return Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = fileUrl, DeleteHash = null });
            }
            else
            {
                _logger.LogError("Failed to upload file to Cloudflare R2. Status: {StatusCode}. Response: {Response}", response.HttpStatusCode, System.Text.Json.JsonSerializer.Serialize(response));
                return Result<FileStorageResultDto>.Failure($"Failed to upload file to Cloudflare R2. Status: {response.HttpStatusCode}", "CloudflareR2UploadError");
            }
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "Cloudflare R2 S3 error during upload: {Message}. InnerException: {@InnerException}", s3Ex.Message, s3Ex.InnerException);
            return Result<FileStorageResultDto>.Failure($"Cloudflare R2 S3 error: {s3Ex.Message}", "CloudflareR2S3Exception");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during R2 upload: {Message}. InnerException: {@InnerException}", ex.Message, ex.InnerException);
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
            _logger.LogWarning("File not found in Cloudflare R2: {FilePath}. S3 Message: {Message}", filePath, s3Ex.Message);
            throw new FileNotFoundException($"File not found in Cloudflare R2: {filePath}", s3Ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve file from Cloudflare R2: {FilePath}. Message: {Message}. InnerException: {InnerException}", filePath, ex.Message, ex.InnerException?.Message);
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
                _logger.LogError("Failed to delete file from Cloudflare R2. Status: {StatusCode}. Response: {Response}", response.HttpStatusCode, System.Text.Json.JsonSerializer.Serialize(response));
                return Result.Failure($"Failed to delete file from Cloudflare R2. Status: {response.HttpStatusCode}", "CloudflareR2DeleteError");
            }
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "Cloudflare R2 S3 error during deletion: {Message}. InnerException: {InnerException}", s3Ex.Message, s3Ex.InnerException?.Message);
            return Result.Failure($"Cloudflare R2 S3 error during deletion: {s3Ex.Message}", "CloudflareR2S3Exception");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during R2 deletion: {Message}. InnerException: {InnerException}", ex.Message, ex.InnerException?.Message);
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
