using backend.Application.AI.VectorStore;
using backend.Application.Common.Interfaces;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries; // For DetectedFaceDto
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler : IRequestHandler<DetectFacesCommand, FaceDetectionResponseDto>
{
    private readonly IFaceApiService _faceApiService; // Changed from IFaceDetectionService
    private readonly IApplicationDbContext _context;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly ILogger<DetectFacesCommandHandler> _logger;

    public DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, IVectorStoreFactory vectorStoreFactory, IOptions<VectorStoreSettings> vectorStoreSettings, ILogger<DetectFacesCommandHandler> logger)
    {
        _faceApiService = faceApiService; // Changed
        _context = context;
        _vectorStoreFactory = vectorStoreFactory;
        _vectorStoreSettings = vectorStoreSettings.Value;
        _logger = logger;
    }

    public async Task<FaceDetectionResponseDto> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        var detectedFacesResult = await _faceApiService.DetectFacesAsync(request.ImageBytes, request.ContentType, request.ReturnCrop); // Changed

        // Generate a unique ImageId for this detection session
        var imageId = Guid.NewGuid();

        // Store the original image or a reference to it if needed for later processing
        // For now, we'll just return the detected faces with the generated ImageId

        var detectedFaceDtos = new List<DetectedFaceDto>();
        IVectorStore vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));
        var collectionName = "family-face-embeddings";

        foreach (var faceResult in detectedFacesResult)
        {
            var detectedFaceDto = new DetectedFaceDto
            {
                Id = Guid.NewGuid().ToString(), // Generate a unique ID for each detected face
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
                DeathYear = null
            };

            if (faceResult.Embedding != null && faceResult.Embedding.Length != 0)
            {
                try
                {
                    var queryResults = await vectorStore.QueryAsync(faceResult.Embedding, 1, [], collectionName, cancellationToken);
                    var bestMatch = queryResults.FirstOrDefault();
                    if (bestMatch != null && bestMatch.Score > 0.8) // Consider a confidence score threshold
                    {
                        _logger.LogInformation("Found a match for face with score {Score}.", bestMatch.Score);
                        if (Guid.TryParse(bestMatch.Metadata.GetValueOrDefault("member_id"), out Guid memberId))
                        {
                            detectedFaceDto.MemberId = memberId;
                            var member = _context.Members.Include(e => e.Family).FirstOrDefault(e => e.Id == memberId);
                            if (member != null)
                            {
                                detectedFaceDto.MemberName = member?.FullName;
                                detectedFaceDto.FamilyId = member?.FamilyId;
                                detectedFaceDto.FamilyName = member?.Family?.Name;
                                detectedFaceDto.BirthYear = member?.DateOfBirth?.Year;
                                detectedFaceDto.DeathYear = member?.DateOfDeath?.Year;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error querying vector store for face detection.");
                }
            }
            detectedFaceDtos.Add(detectedFaceDto);
        }

        return new FaceDetectionResponseDto
        {
            ImageId = imageId,
            DetectedFaces = detectedFaceDtos
        };
    }
}
