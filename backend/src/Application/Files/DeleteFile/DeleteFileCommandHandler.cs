using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Files.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public DeleteFileCommandHandler(IApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var fileMetadata = await _context.FileMetadata
            .FirstOrDefaultAsync(fm => fm.Id == request.FileId, cancellationToken);

        if (fileMetadata == null)
        {
            return Result.Failure("File metadata not found.", "NotFound");
        }

        // Delete the actual file from storage
        var deleteResult = await _fileStorageService.DeleteFileAsync(fileMetadata.Url, cancellationToken);
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
