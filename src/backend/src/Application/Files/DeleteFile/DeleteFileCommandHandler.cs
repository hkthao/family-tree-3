using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Files.DeleteFile;

public class DeleteFileCommandHandler(IApplicationDbContext context, IFileStorage fileStorage, ICurrentUser currentUser, IDateTime dateTime, IAuthorizationService authorizationService) : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IFileStorage _fileStorage = fileStorage;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var fileMetadata = await _context.FileMetadata.FindAsync(request.FileId, cancellationToken);

        if (fileMetadata == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, "File metadata"), ErrorSources.NotFound);
        }

        if (fileMetadata.CreatedBy != _currentUser.UserId.ToString() && !_authorizationService.IsAdmin())
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        // For files, we still want to delete the actual file from storage
        var deleteResult = await _fileStorage.DeleteFileAsync(fileMetadata.Url, cancellationToken);

        if (deleteResult.IsSuccess)
        {
            fileMetadata.IsDeleted = true;
            fileMetadata.DeletedDate = _dateTime.Now;
            fileMetadata.DeletedBy = _currentUser.UserId.ToString();
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        else
        {
            return Result.Failure(deleteResult.Error!, deleteResult.ErrorSource!);
        }
    }
}
