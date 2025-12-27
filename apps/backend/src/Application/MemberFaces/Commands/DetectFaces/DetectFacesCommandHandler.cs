using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Queries.SearchVectorFace;
using backend.Application.Members.Specifications;
using Microsoft.Extensions.Logging;
using backend.Application.Common.Utils;

namespace backend.Application.MemberFaces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, ILogger<DetectFacesCommandHandler> logger, IMediator mediator, ICurrentUser currentUser, IAuthorizationService authorizationService) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<FaceDetectionResponseDto>> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;
            var isAdmin = _authorizationService.IsAdmin();
            var memberAccessSpec = new MemberAccessSpecification(isAdmin, currentUserId);

            string effectiveFileName = string.IsNullOrWhiteSpace(request.FileName)
                ? $"uploaded_image_{Guid.NewGuid()}{Path.GetExtension(request.ContentType)}"
                : request.FileName;
            string? originalImageUrl = null;


            string uploadFolder = UploadConstants.TemporaryUploadsFolder;

            int? imageWidth = null;
            int? imageHeight = null;

            if (request.ImageBytes != null && request.ImageBytes.Length > 0)
            {
                // Identify image dimensions
                var (width, height) = ImageUtils.GetImageDimensions(request.ImageBytes, _logger);
                imageWidth = width;
                imageHeight = height;

                var originalUploadCommand = new UploadFileCommand
                {
                    ImageData = request.ImageBytes,
                    FileName = effectiveFileName,
                    Folder = uploadFolder,
                    ContentType = request.ContentType
                };
                var originalUploadResult = await _mediator.Send(originalUploadCommand, cancellationToken);
                if (originalUploadResult.IsSuccess && originalUploadResult.Value != null)
                {
                    originalImageUrl = originalUploadResult.Value.Url;
                }
                else
                {
                    return Result<FaceDetectionResponseDto>.Failure(originalUploadResult.Error ?? ErrorMessages.FileUploadFailed);
                }

            }
            var detectedFacesResult = await _faceApiService.DetectFacesAsync(request.ImageBytes!, request.ContentType, request.ReturnCrop);
            var imageId = Guid.NewGuid();
            var detectedFaceDtos = new List<DetectedFaceDto>();
            var memberIdsToFetch = new HashSet<Guid>();
            var faceMemberMap = new Dictionary<string, Guid>();
            foreach (var faceResult in detectedFacesResult)
            {
                var detectedFaceDto = new DetectedFaceDto
                {
                    Id = Guid.NewGuid().ToString(),
                    BoundingBox = new BoundingBoxDto
                    {
                        X = faceResult.BoundingBox.X,
                        Y = faceResult.BoundingBox.Y,
                        Width = faceResult.BoundingBox.Width,
                        Height = faceResult.BoundingBox.Height
                    },
                    Confidence = faceResult.Confidence,
                    Thumbnail = faceResult.Thumbnail,
                    Embedding = faceResult.Embedding?.ToList(),
                    MemberId = null,
                    MemberName = null,
                    FamilyId = null,
                    FamilyName = null,
                    BirthYear = null,
                    DeathYear = null,
                    Emotion = faceResult.Emotion,
                    EmotionConfidence = faceResult.EmotionConfidence,
                    Status = "unrecognized"
                };
                if (detectedFaceDto.Embedding != null && detectedFaceDto.Embedding.Any())
                {
                    var searchFaceQuery = new SearchMemberFaceQuery(request.FamilyId)
                    {
                        Vector = detectedFaceDto.Embedding,
                        Limit = 1,
                        Threshold = 0.7f
                    };

                    var searchResult = await _mediator.Send(searchFaceQuery, cancellationToken);
                    if (searchResult.IsSuccess && searchResult.Value != null && searchResult.Value.Any())
                    {
                        var foundFace = searchResult.Value.First();
                        detectedFaceDto.MemberId = foundFace.MemberId;
                        memberIdsToFetch.Add(foundFace.MemberId);
                        faceMemberMap[detectedFaceDto.Id] = foundFace.MemberId;
                    }
                    else if (!searchResult.IsSuccess)
                    {
                        _logger.LogError("Failed to search face embedding for FaceId {FaceId}: {Error}", detectedFaceDto.Id, searchResult.Error);
                    }
                    else
                    {
                        _logger.LogInformation("No matching face found for FaceId {FaceId}.", detectedFaceDto.Id);
                    }
                }
                detectedFaceDto.Status = detectedFaceDto.MemberId.HasValue ? "recognized" : "unrecognized";
                detectedFaceDtos.Add(detectedFaceDto);
            }
            if (memberIdsToFetch.Any())
            {

                var members = await _context.Members
                    .WithSpecification(memberAccessSpec)
                    .Where(m => memberIdsToFetch.Contains(m.Id))
                    .Include(m => m.Family)
                    .ToListAsync(cancellationToken);


                detectedFaceDtos = detectedFaceDtos.Where(df => !df.MemberId.HasValue || members.Any(m => m.Id == df.MemberId.Value)).ToList();

                foreach (var faceDto in detectedFaceDtos)
                {
                    if (faceDto.MemberId.HasValue)
                    {
                        var member = members.FirstOrDefault(m => m.Id == faceDto.MemberId.Value);
                        if (member != null)
                        {
                            faceDto.MemberName = member.FullName;
                            faceDto.FamilyId = member.FamilyId;
                            faceDto.FamilyName = member.Family?.Name;
                            faceDto.BirthYear = member.DateOfBirth?.Year;
                            faceDto.DeathYear = member.DateOfDeath?.Year;
                        }
                    }
                }
            }
            return Result<FaceDetectionResponseDto>.Success(new FaceDetectionResponseDto
            {
                ImageId = imageId,
                OriginalImageUrl = originalImageUrl,
                ResizedImageUrl = null,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                DetectedFaces = detectedFaceDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during face detection.");
            return Result<FaceDetectionResponseDto>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message));
        }
    }
}
