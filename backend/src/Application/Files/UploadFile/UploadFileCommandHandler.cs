using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using System.Text.RegularExpressions;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Files.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<string>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IStorageSettings _storageSettings;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IDateTime _dateTime;

    public UploadFileCommandHandler(IFileStorageService fileStorageService, IStorageSettings storageSettings, IApplicationDbContext context, IUser user, IDateTime dateTime)
    {
        _fileStorageService = fileStorageService;
        _storageSettings = storageSettings;
        _context = context;
        _user = user;
        _dateTime = dateTime;
    }

    public async Task<Result<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate file size
        var maxFileSizeInBytes = _storageSettings.MaxFileSizeMB * 1024 * 1024;
        if (request.Length == 0)
        {
            return Result<string>.Failure("File is empty.", "Validation");
        }
        if (request.Length > maxFileSizeInBytes)
        {
            return Result<string>.Failure($"File size exceeds the maximum limit of {_storageSettings.MaxFileSizeMB} MB.", "Validation");
        }

        // 2. Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Result<string>.Failure("Invalid file type. Only JPG, JPEG, PNG, PDF, DOCX are allowed.", "Validation");
        }

        // 3. Sanitize file name to prevent path traversal
        var sanitizedFileName = SanitizeFileName(request.FileName);
        // Add a unique identifier to prevent overwriting
        var uniqueFileName = $"{Path.GetFileNameWithoutExtension(sanitizedFileName)}_{Guid.NewGuid()}{fileExtension}";

        // 4. Upload file
        await using (request.FileStream)
        {
            var uploadResult = await _fileStorageService.UploadFileAsync(request.FileStream, uniqueFileName, request.ContentType, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return Result<string>.Failure(uploadResult.Error ?? "File upload failed.", uploadResult.ErrorSource ?? "FileStorage");
            }
            if (uploadResult.Value == null)
            {
                return Result<string>.Failure("File upload succeeded but returned a null URL.", "FileStorage");
            }

            // 5. Save file metadata to DB
            var fileMetadata = new FileMetadata
            {
                FileName = uniqueFileName,
                Url = uploadResult.Value,
                StorageProvider = Enum.Parse<StorageProvider>(_storageSettings.Provider, true),
                ContentType = request.ContentType,
                FileSize = request.Length,
                UploadedBy = _user.Id ?? "",
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
