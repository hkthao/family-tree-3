using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandHandler : IRequestHandler<CreateFamilyMediaCommand, Result<FamilyMediaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateFamilyMediaCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateFamilyMediaCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IFileStorageService fileStorageService,
        ICurrentUser currentUser,
        ILogger<CreateFamilyMediaCommandHandler> logger,
        IMapper mapper,
        IMediator mediator)
    {
        _context = context;
        _authorizationService = authorizationService;
        _fileStorageService = fileStorageService;
        _currentUser = currentUser;
        _logger = logger;
        _mapper = mapper;
        _mediator = mediator;
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

        // --- Storage Limit Check ---
        var familyLimitConfigResult = await _mediator.Send(new Families.Queries.GetFamilyLimitConfigurationQuery { FamilyId = request.FamilyId });

        if (!familyLimitConfigResult.IsSuccess)
        {
            _logger.LogError("Failed to retrieve family limit configuration for family ID {FamilyId}: {Error}", request.FamilyId, familyLimitConfigResult.Error);
            return Result<FamilyMediaDto>.Failure("Failed to retrieve family storage limits.", ErrorSources.InternalError);
        }

        var familyLimitConfig = familyLimitConfigResult.Value;

        if (familyLimitConfig == null) // Should not happen if query returns default
        {
            _logger.LogError("FamilyLimitConfiguration is null for family ID {FamilyId}", request.FamilyId);
            return Result<FamilyMediaDto>.Failure("Family storage limits not found.", ErrorSources.InternalError);
        }

        long maxStorageBytes = (long)familyLimitConfig.MaxStorageMb * 1024 * 1024; // Convert MB to bytes

        var currentStorageBytes = await _context.FamilyMedia
            .Where(fm => fm.FamilyId == request.FamilyId)
            .SumAsync(fm => fm.FileSize, cancellationToken);

        if (currentStorageBytes + request.File.Length > maxStorageBytes)
        {
            return Result<FamilyMediaDto>.Failure($"Storage limit ({familyLimitConfig.MaxStorageMb} MB) exceeded.", ErrorSources.Validation);
        }

        // --- End Storage Limit Check ---

        string fileNameInStorage = $"{Guid.NewGuid()}{Path.GetExtension(request.FileName)}"; // Use original file extension from FileName property

        string folderToUpload = request.Folder ?? string.Empty;
        if (string.IsNullOrWhiteSpace(folderToUpload))
        {
            var mediaType = request.MediaType ?? request.FileName.InferMediaType();
            switch (mediaType)
            {
                case Domain.Enums.MediaType.Image:
                    folderToUpload = string.Format(UploadConstants.ImagesFolder, request.FamilyId);
                    break;
                case Domain.Enums.MediaType.Video:
                    folderToUpload = string.Format(UploadConstants.VideosFolder, request.FamilyId);
                    break;
                default:
                    folderToUpload = string.Format(UploadConstants.ImagesFolder, request.FamilyId); // Fallback
                    break;
            }
        }

        // Determine content type based on inferred or provided MediaType
        var actualMediaType = request.MediaType ?? request.FileName.InferMediaType();
        var contentType = GetContentTypeFromMediaType(actualMediaType);

        using var fileStream = new MemoryStream(request.File); // Create MemoryStream from byte array
        var uploadResult = await _fileStorageService.UploadFileAsync(
            fileStream,
            fileNameInStorage,
            contentType, // New argument
            folderToUpload,
            cancellationToken
        );

        if (!uploadResult.IsSuccess)
        {
            _logger.LogError("File upload failed: {Error}", uploadResult.Error);
            return Result<FamilyMediaDto>.Failure(uploadResult.Error ?? "File upload failed.", ErrorSources.ExternalServiceError);
        }

        if (uploadResult.Value == null || string.IsNullOrEmpty(uploadResult.Value.FileUrl))
        {
            return Result<FamilyMediaDto>.Failure("File upload failed, no valid URL returned.", ErrorSources.ExternalServiceError);
        }

        var familyMedia = new Domain.Entities.FamilyMedia
        {
            FamilyId = request.FamilyId,
            FileName = request.FileName, // Store original file name
            FilePath = uploadResult.Value.FileUrl!, // The URL returned by the storage service
            MediaType = request.MediaType ?? request.FileName.InferMediaType(), // Infer from FileName
            FileSize = request.File.Length,
            Description = request.Description,
            UploadedBy = _currentUser.UserId, // Current user uploading the file
            DeleteHash = uploadResult.Value.DeleteHash // Store DeleteHash from Imgur (if any)
        };

        _context.FamilyMedia.Add(familyMedia);
        await _context.SaveChangesAsync(cancellationToken);

        var familyMediaDto = _mapper.Map<FamilyMediaDto>(familyMedia);
        return Result<FamilyMediaDto>.Success(familyMediaDto);
    }

    private static string GetContentTypeFromMediaType(backend.Domain.Enums.MediaType mediaType)
    {
        return mediaType switch
        {
            backend.Domain.Enums.MediaType.Image => "image/jpeg", // Mặc định là jpeg, lý tưởng nên cụ thể hơn
            backend.Domain.Enums.MediaType.Video => "video/mp4",  // Mặc định là mp4
            backend.Domain.Enums.MediaType.Audio => "audio/mpeg", // Mặc định là mpeg
            backend.Domain.Enums.MediaType.Document => "application/pdf", // Chỉ loại tài liệu được xử lý rõ ràng
            _ => "application/octet-stream" // Dữ liệu nhị phân chung
        };
    }
}
