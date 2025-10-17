using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Files.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IUser _user;

    public DeleteFileCommandHandler(IApplicationDbContext context, IFileStorage fileStorage, IUser user)
    {
        _context = context;
        _fileStorage = fileStorage;
        _user = user;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var fileMetadata = await _context.FileMetadata
            .FirstOrDefaultAsync(fm => fm.Id == request.FileId, cancellationToken);

        if (fileMetadata == null)
        {
            return Result.Failure("File metadata not found.", "NotFound");
        }

        if (fileMetadata.UploadedBy != _user.Id)
        {
            return Result.Failure("User is not authorized to delete this file.", "Forbidden");
        }

        // Delete the actual file from storage
        var deleteResult = await _fileStorage.DeleteFileAsync(fileMetadata.Url, cancellationToken);
        if (!deleteResult.IsSuccess)
        {
            return Result.Failure(deleteResult.Error ?? "Failed to delete file from storage.", deleteResult.ErrorSource ?? "FileStorage");
        }

        // Remove metadata record from DB
        _context.FileMetadata.Remove(fileMetadata);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
