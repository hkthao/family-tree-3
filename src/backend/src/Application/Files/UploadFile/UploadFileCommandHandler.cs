using System.Text.RegularExpressions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Files.UploadFile;

public class UploadFileCommandHandler(IFileStorage fileStorage, IConfigProvider configProvider, IApplicationDbContext context, IUser user, IDateTime dateTime) : IRequestHandler<UploadFileCommand, Result<string>>
{
    private readonly IFileStorage _fileStorage = fileStorage;
    private readonly IConfigProvider _configProvider = configProvider;
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var storageSettings = _configProvider.GetSection<StorageSettings>();
        // 1. Validate file size
        var maxFileSizeInBytes = storageSettings.MaxFileSizeMB * 1024 * 1024;
        if (request.Length == 0)
        {
            return Result<string>.Failure(ErrorMessages.FileEmpty, ErrorSources.Validation);
        }
        if (request.Length > maxFileSizeInBytes)
        {
            return Result<string>.Failure(string.Format(ErrorMessages.FileSizeExceedsLimit, storageSettings.MaxFileSizeMB), ErrorSources.Validation);
        }

        // 2. Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Result<string>.Failure(ErrorMessages.InvalidFileType, ErrorSources.Validation);
        }

        // 3. Sanitize file name to prevent path traversal
        var sanitizedFileName = SanitizeFileName(request.FileName);
        // Add a unique identifier to prevent overwriting
        var uniqueFileName = $"{Path.GetFileNameWithoutExtension(sanitizedFileName)}_{Guid.NewGuid()}{fileExtension}";

        // 4. Upload file
        await using (request.FileStream)
        {
            var uploadResult = await _fileStorage.UploadFileAsync(request.FileStream, uniqueFileName, request.ContentType, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return Result<string>.Failure(uploadResult.Error ?? ErrorMessages.FileUploadFailed, uploadResult.ErrorSource ?? ErrorSources.FileStorage);
            }
            if (uploadResult.Value == null)
            {
                return Result<string>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileStorage);
            }

            // 5. Save file metadata to DB
            var fileMetadata = new FileMetadata
            {
                FileName = uniqueFileName,
                Url = uploadResult.Value,
                StorageProvider = Enum.Parse<StorageProvider>(storageSettings.Provider, true),
                ContentType = request.ContentType,
                FileSize = request.Length,
                UploadedBy = _user.Id?.ToString() ?? "",
                IsActive = true,
                Created = _dateTime.Now,
                LastModified = _dateTime.Now
            };

            _context.FileMetadata.Add(fileMetadata);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(uploadResult.Value);
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove any path traversal attempts (e.g., ../)
        fileName = Path.GetFileName(fileName);
        // Remove invalid characters for file names
        fileName = Regex.Replace(fileName, "[^a-zA-Z0-9_.-]", "");
        return fileName;
    }
}
