using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Files.DeleteFile;

public class DeleteFileCommandHandler(IApplicationDbContext context, IFileStorage fileStorage, IUser currentUser, IDateTime dateTime) : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IFileStorage _fileStorage = fileStorage;
    private readonly IUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var fileMetadata = await _context.FileMetadata
            .FirstOrDefaultAsync(fm => fm.Id == request.FileId, cancellationToken);

        if (fileMetadata == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, "File metadata"), ErrorSources.NotFound);
        }

        if (fileMetadata.UploadedBy != _currentUser.Id?.ToString())
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // For files, we still want to delete the actual file from storage
        var deleteResult = await _fileStorage.DeleteFileAsync(fileMetadata.Url, cancellationToken);
        if (!deleteResult.IsSuccess)
        {
            return Result.Failure(deleteResult.Error ?? string.Format(ErrorMessages.UnexpectedError, "file deletion from storage"), deleteResult.ErrorSource ?? ErrorSources.FileStorage);
        }

        // Soft delete metadata record from DB
        fileMetadata.IsDeleted = true;
        fileMetadata.DeletedBy = _currentUser.Id?.ToString();
        fileMetadata.DeletedDate = _dateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
