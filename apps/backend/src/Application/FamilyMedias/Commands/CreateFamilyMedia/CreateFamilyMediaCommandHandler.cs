using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.MessageBus; // NEW
using backend.Application.FamilyMedias.DTOs;
using Microsoft.Extensions.Logging;
using static backend.Application.Common.Constants.MessageBusConstants;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandHandler : IRequestHandler<CreateFamilyMediaCommand, Result<FamilyMediaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileStorageService _fileStorageService; // Changed from ILocalFileStorageService
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateFamilyMediaCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IMessageBus _messageBus;

    public CreateFamilyMediaCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IFileStorageService fileStorageService, // Changed from ILocalFileStorageService
        ICurrentUser currentUser,
        ILogger<CreateFamilyMediaCommandHandler> logger,
        IMapper mapper,
        IMediator mediator,
        IMessageBus messageBus)
    {
        _context = context;
        _authorizationService = authorizationService;
        _fileStorageService = fileStorageService; // Changed from _localFileStorageService
        _currentUser = currentUser;
        _logger = logger;
        _mapper = mapper;
        _mediator = mediator;
        _messageBus = messageBus;
    }

    public async Task<Result<FamilyMediaDto>> Handle(CreateFamilyMediaCommand request, CancellationToken cancellationToken)
    {
        if (!request.FamilyId.HasValue || !_authorizationService.CanManageFamily(request.FamilyId.Value))
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
        var familyLimitConfigResult = await _mediator.Send(new Families.Queries.GetFamilyLimitConfigurationQuery { FamilyId = request.FamilyId.Value });

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

        Guid familyId = request.FamilyId.Value; // Get the non-nullable Guid
        var currentStorageBytes = await _context.FamilyMedia
            .Where(fm => fm.FamilyId == familyId)
            .SumAsync(fm => fm.FileSize, cancellationToken);

        if (currentStorageBytes + request.File.Length > maxStorageBytes)
        {
            return Result<FamilyMediaDto>.Failure($"Storage limit ({familyLimitConfig.MaxStorageMb} MB) exceeded.", ErrorSources.Validation);
        }

        // --- End Storage Limit Check ---

        // Determine content type based on inferred or provided MediaType
        var actualMediaType = request.MediaType ?? request.FileName.InferMediaType();
        // The contentType from request should be used, or inferred if null/empty
        var effectiveContentType = request.ContentType;
        if (string.IsNullOrWhiteSpace(effectiveContentType))
        {
            effectiveContentType = GetContentTypeFromMediaType(actualMediaType);
        }

        // Determine the target folder for Cloudinary based on family ID and media type
        var mediaTypeFolder = request.MediaType switch
        {
            Domain.Enums.MediaType.Image => "images",
            Domain.Enums.MediaType.Video => "videos",
            Domain.Enums.MediaType.Document => "documents",
            _ => "others"
        };
        var targetFolder = $"{request.FamilyId.Value}/{mediaTypeFolder}";

        // Save file locally first using the new IFileStorageService (LocalDiskFileStorageService)
        using var fileStream = new MemoryStream(request.File);
        var saveResult = await _fileStorageService.SaveFileAsync(
            fileStream,
            request.FileName,
            effectiveContentType,
            targetFolder, // Use the dynamically generated targetFolder
            cancellationToken
        );

        if (!saveResult.IsSuccess || saveResult.Value == null || string.IsNullOrEmpty(saveResult.Value.FileUrl))
        {
            _logger.LogError("Local file save failed: {Error}", saveResult.Error);
            return Result<FamilyMediaDto>.Failure(saveResult.Error ?? "Local file save failed.", saveResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        var tempLocalPath = saveResult.Value.FileUrl; // FileUrl now holds the local path

        // Create FamilyMedia entity with temporary local path
        var familyMedia = new Domain.Entities.FamilyMedia
        {
            FamilyId = familyId,
            FileName = request.FileName, // Store original file name
            FilePath = tempLocalPath, // Store temporary local path
            MediaType = actualMediaType,
            FileSize = request.File.Length,
            Description = request.Description,
            UploadedBy = _currentUser.UserId,
            // DeleteHash will be set later by storage-service if needed
        };

        _context.FamilyMedia.Add(familyMedia);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish message to RabbitMQ for storage-service to pick up
        var fileUploadEvent = new FileUploadRequestedEvent
        {
            FileId = familyMedia.Id,
            OriginalFileName = request.FileName,
            TempLocalPath = tempLocalPath,
            ContentType = effectiveContentType,
            Folder = targetFolder,
            UploadedBy = _currentUser.UserId,
            FileSize = request.File.Length,
            FamilyId = request.FamilyId // Assign the nullable Guid directly
        };
        await _messageBus.PublishAsync(MessageBusConstants.Exchanges.FileUpload, MessageBusConstants.RoutingKeys.FileUploadRequested, fileUploadEvent);

        _logger.LogInformation("File {FileName} saved locally at {TempPath} and FileUploadRequestedEvent published for FamilyMedia ID {FamilyMediaId}.",
                               request.FileName, tempLocalPath, familyMedia.Id);

        var familyMediaDto = _mapper.Map<FamilyMediaDto>(familyMedia);
        // It's important that this DTO now contains the temporary local path, not the final cloud URL.
        // The client will need to handle this or poll for updates.
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
