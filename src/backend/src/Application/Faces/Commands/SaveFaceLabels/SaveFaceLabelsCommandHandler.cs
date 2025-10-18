using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandler : IRequestHandler<SaveFaceLabelsCommand, Result<Unit>>
{
    private readonly ILogger<SaveFaceLabelsCommandHandler> _logger;
    private readonly IFaceApiService _faceApiService; // Assuming this service exists for face embedding
    private readonly IVectorStoreService _vectorStoreService; // Assuming this service exists for vector storage

    public SaveFaceLabelsCommandHandler(
        ILogger<SaveFaceLabelsCommandHandler> logger,
        IFaceApiService faceApiService,
        IVectorStoreService vectorStoreService)
    {
        _logger = logger;
        _faceApiService = faceApiService;
        _vectorStoreService = vectorStoreService;
    }

    public async Task<Result<Unit>> Handle(SaveFaceLabelsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SaveFaceLabelsCommand for ImageId {ImageId} with {FaceCount} faces.",
            request.ImageId, request.FaceLabels.Count);

        foreach (var faceLabel in request.FaceLabels)
        {
            if (faceLabel.Thumbnail == null)
            {
                _logger.LogWarning("Face {FaceId} has no thumbnail. Skipping embedding generation.", faceLabel.Id);
                continue;
            }

            // 1. Get face embedding from Face API Service
            // The thumbnail is base64 encoded, so pass it directly
            var embeddingResult = await _faceApiService.GetFaceEmbeddingAsync(faceLabel.Thumbnail, cancellationToken);

            if (!embeddingResult.IsSuccess)
            {
                _logger.LogError("Failed to get embedding for face {FaceId}: {Error}", faceLabel.Id, embeddingResult.Error);
                return Result<Unit>.Failure($"Failed to get embedding for face {faceLabel.Id}: {embeddingResult.Error}");
            }

            // Ensure embedding is not null before proceeding
            if (embeddingResult.Value == null)
            {
                _logger.LogError("Embedding result value is null for face {FaceId}.", faceLabel.Id);
                return Result<Unit>.Failure($"Embedding result value is null for face {faceLabel.Id}.");
            }

            var embedding = embeddingResult.Value;

            // 2. Prepare metadata for vector store
            var metadata = new Dictionary<string, string>
            {
                { "FaceId", faceLabel.Id },
                { "ImageId", request.ImageId.ToString() },
                { "MemberId", faceLabel.MemberId?.ToString() ?? string.Empty },
                { "MemberName", faceLabel.MemberName ?? string.Empty },
                { "FamilyId", faceLabel.FamilyId?.ToString() ?? string.Empty },
                { "FamilyName", faceLabel.FamilyName ?? string.Empty },
                { "BirthYear", faceLabel.BirthYear?.ToString() ?? string.Empty },
                { "DeathYear", faceLabel.DeathYear?.ToString() ?? string.Empty },
                // Add bounding box coordinates as separate metadata fields if needed for querying
                { "BoundingBox_X", faceLabel.BoundingBox.X.ToString() },
                { "BoundingBox_Y", faceLabel.BoundingBox.Y.ToString() },
                { "BoundingBox_Width", faceLabel.BoundingBox.Width.ToString() },
                { "BoundingBox_Height", faceLabel.BoundingBox.Height.ToString() }
            };

            // 3. Save embedding and metadata to vector store
            var saveResult = await _vectorStoreService.SaveVectorAsync(embedding, metadata, cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Failed to save vector for face {FaceId}: {Error}", faceLabel.Id, saveResult.Error);
                return Result<Unit>.Failure($"Failed to save vector for face {faceLabel.Id}: {saveResult.Error}");
            }
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
