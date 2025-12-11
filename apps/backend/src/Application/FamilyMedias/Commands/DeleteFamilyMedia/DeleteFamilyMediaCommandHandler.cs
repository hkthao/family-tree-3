using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging; // ADDED

namespace backend.Application.FamilyMedias.Commands.DeleteFamilyMedia;

public class DeleteFamilyMediaCommandHandler : IRequestHandler<DeleteFamilyMediaCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteFamilyMediaCommandHandler> _logger;

    public DeleteFamilyMediaCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IFileStorageService fileStorageService,
        ILogger<DeleteFamilyMediaCommandHandler> logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFamilyMediaCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var familyMedia = await _context.FamilyMedia
            .FirstOrDefaultAsync(fm => fm.Id == request.Id && fm.FamilyId == request.FamilyId && !fm.IsDeleted, cancellationToken);

        if (familyMedia == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.Id}"), ErrorSources.NotFound);
        }

        // Attempt to delete the file from storage
        var deleteFileResult = await _fileStorageService.DeleteFileAsync(familyMedia.FilePath, cancellationToken);
        if (!deleteFileResult.IsSuccess)
        {
            _logger.LogWarning("Failed to delete file from storage for FamilyMedia ID {FamilyMediaId}: {Error}", familyMedia.Id, deleteFileResult.Error);
            // Optionally, you might choose to return failure here or proceed with DB deletion anyway
            // For now, we'll proceed with DB deletion even if storage deletion fails,
            // to allow manual cleanup of orphaned files in storage if necessary.
        }

        _context.FamilyMedia.Remove(familyMedia); // Soft delete is handled by interceptor

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
