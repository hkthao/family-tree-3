using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.MemberFaces.Common;
using backend.Application.Members.Specifications;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, ILogger<DetectFacesCommandHandler> logger, ICurrentUser currentUser, IAuthorizationService authorizationService) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;
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

            string uploadFolder = string.Format(UploadConstants.FaceImagesFolder, request.FamilyId);

            int? imageWidth = null;
            int? imageHeight = null;

            if (request.ImageBytes != null && request.ImageBytes.Length > 0)
            {
                // Identify image dimensions
                var (width, height) = ImageUtils.GetImageDimensions(request.ImageBytes, _logger);
                imageWidth = width;
                imageHeight = height;
            }
            var detectedFacesResult = await _faceApiService.DetectFacesAsync(request.ImageBytes!, request.ContentType, request.ReturnCrop);
            var imageId = Guid.NewGuid();
            var detectedFaceDtos = new List<DetectedFaceDto>();
            var memberIdsToFetch = new HashSet<Guid>();
            var faceMemberMap = new Dictionary<string, Guid>();

            // Collect all embeddings for batch search
            var embeddingsToSearch = new List<List<float>>();
            var detectedFaceDtoMapping = new List<DetectedFaceDto>(); // To map back after batch search

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
                    Embedding = faceResult.Embedding?.Select(d => (float)d).ToList(),
                    MemberId = null,
                    MemberName = null,
                    FamilyId = null,
                    FamilyName = null,
                    BirthYear = null,
                    DeathYear = null,
                    Status = "unrecognized"
                };

                detectedFaceDtos.Add(detectedFaceDto); // Add to the main list immediately
                if (detectedFaceDto.Embedding != null && detectedFaceDto.Embedding.Any())
                {
                    embeddingsToSearch.Add(detectedFaceDto.Embedding);
                    detectedFaceDtoMapping.Add(detectedFaceDto); // Keep track of which DTO corresponds to which embedding
                }
            }

            if (embeddingsToSearch.Any())
            {
                var batchSearchRequest = new BatchFaceSearchVectorRequestDto
                {
                    Vectors = embeddingsToSearch,
                    FamilyId = request.FamilyId,
                    Limit = 1, // We only need the top match
                    Threshold = 0.7f // Default threshold
                };

                var batchSearchResults = await _faceApiService.BatchSearchSimilarFacesAsync(batchSearchRequest);

                for (int i = 0; i < batchSearchResults.Count; i++)
                {
                    var searchResult = batchSearchResults[i];
                    var originalDetectedFaceDto = detectedFaceDtoMapping[i];

                    if (searchResult != null && searchResult.Any())
                    {
                        var foundFace = searchResult.First();
                        if (foundFace.Payload?.MemberId != null && foundFace.Payload.MemberId != Guid.Empty)
                        {
                            originalDetectedFaceDto.MemberId = foundFace.Payload.MemberId;
                            memberIdsToFetch.Add(foundFace.Payload.MemberId);
                            faceMemberMap[originalDetectedFaceDto.Id] = foundFace.Payload.MemberId;
                            originalDetectedFaceDto.Status = "recognized";
                        }
                    }
                }
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
                OriginalImageUrl = string.Empty,
                ResizedImageUrl = null,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                DetectedFaces = detectedFaceDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during face detection: {ErrorMessage}", ex.Message);
            return Result<FaceDetectionResponseDto>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message));
        }
    }
}
