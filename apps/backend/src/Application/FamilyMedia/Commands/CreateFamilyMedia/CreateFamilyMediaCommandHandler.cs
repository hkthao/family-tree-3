using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.IO; // Required for MemoryStream

namespace backend.Application.FamilyMedia.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandHandler : IRequestHandler<CreateFamilyMediaCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateFamilyMediaCommandHandler> _logger;

    public CreateFamilyMediaCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IFileStorageService fileStorageService,
        ICurrentUser currentUser,
        ILogger<CreateFamilyMediaCommandHandler> logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _fileStorageService = fileStorageService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateFamilyMediaCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        if (request.File == null || request.File.Length == 0)
        {
            return Result<Guid>.Failure("File content is empty.", ErrorSources.Validation);
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Result<Guid>.Failure("File name is empty.", ErrorSources.Validation);
        }

        string folderPath = Path.Combine("family-media", request.FamilyId.ToString(), request.Folder ?? "");
        string fileNameInStorage = $"{Guid.NewGuid()}{Path.GetExtension(request.FileName)}"; // Use original file extension from FileName property

        using var fileStream = new MemoryStream(request.File); // Create MemoryStream from byte array
        var uploadResult = await _fileStorageService.UploadFileAsync(
            fileStream,
            fileNameInStorage,
            folderPath,
            cancellationToken
        );

        if (!uploadResult.IsSuccess)
        {
            _logger.LogError("File upload failed: {Error}", uploadResult.Error);
            return Result<Guid>.Failure(uploadResult.Error ?? "File upload failed.", ErrorSources.ExternalServiceError);
        }

        var familyMedia = new backend.Domain.Entities.FamilyMedia
        {
            FamilyId = request.FamilyId,
            FileName = request.FileName, // Store original file name
            FilePath = uploadResult.Value!, // The URL returned by the storage service
            MediaType = request.MediaType ?? request.FileName.InferMediaType(), // Infer from FileName
            FileSize = request.File.Length,
            Description = request.Description,
            UploadedBy = _currentUser.UserId // Current user uploading the file
        };

        _context.FamilyMedia.Add(familyMedia);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(familyMedia.Id);
    }
}
