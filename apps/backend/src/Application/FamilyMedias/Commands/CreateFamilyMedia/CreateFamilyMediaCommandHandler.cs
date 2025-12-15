using System.IO; // Required for MemoryStream
using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using AutoMapper;
using backend.Application.FamilyMedias.DTOs;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandHandler : IRequestHandler<CreateFamilyMediaCommand, Result<FamilyMediaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateFamilyMediaCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateFamilyMediaCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IFileStorageService fileStorageService,
        ICurrentUser currentUser,
        ILogger<CreateFamilyMediaCommandHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _fileStorageService = fileStorageService;
        _currentUser = currentUser;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<FamilyMediaDto>> Handle(CreateFamilyMediaCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<FamilyMediaDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        if (request.File == null || request.File.Length == 0)
        {
            return Result<FamilyMediaDto>.Failure("File content is empty.", ErrorSources.Validation);
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Result<FamilyMediaDto>.Failure("File name is empty.", ErrorSources.Validation);
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
            return Result<FamilyMediaDto>.Failure(uploadResult.Error ?? "File upload failed.", ErrorSources.ExternalServiceError);
        }

        var familyMedia = new Domain.Entities.FamilyMedia
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

        var familyMediaDto = _mapper.Map<FamilyMediaDto>(familyMedia);
        return Result<FamilyMediaDto>.Success(familyMediaDto);
    }
}
