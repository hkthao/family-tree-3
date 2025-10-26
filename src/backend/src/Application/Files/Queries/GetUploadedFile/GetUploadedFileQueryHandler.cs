using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;

namespace backend.Application.Files.Queries.GetUploadedFile;

public class GetUploadedFileQueryHandler(IConfigProvider configProvider) : IRequestHandler<GetUploadedFileQuery, Result<FileContentDto>>
{
    private readonly IConfigProvider _configProvider = configProvider;

    public Task<Result<FileContentDto>> Handle(GetUploadedFileQuery request, CancellationToken cancellationToken)
    {
        var storageSettings = _configProvider.GetSection<StorageSettings>();
        // Sanitize fileName to prevent path traversal
        var sanitizedFileName = Path.GetFileName(request.FileName);
        var filePath = Path.Combine(storageSettings.Local.LocalStoragePath, sanitizedFileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult(Result<FileContentDto>.Failure(string.Format(ErrorMessages.NotFound, "File"), ErrorSources.NotFound));
        }

        // Determine content type
        var contentType = "application/octet-stream"; // Default
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        switch (ext)
        {
            case ".jpg":
            case ".jpeg":
                contentType = "image/jpeg";
                break;
            case ".png":
                contentType = "image/png";
                break;
            case ".pdf":
                contentType = "application/pdf";
                break;
            case ".docx":
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                break;
        }

        var fileContent = File.ReadAllBytes(filePath);

        return Task.FromResult(Result<FileContentDto>.Success(new FileContentDto
        {
            Content = fileContent,
            ContentType = contentType
        }));
    }
}
