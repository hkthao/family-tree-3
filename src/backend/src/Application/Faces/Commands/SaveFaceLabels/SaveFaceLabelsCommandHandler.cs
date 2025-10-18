using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using backend.Domain.Entities; // Added

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandler : IRequestHandler<SaveFaceLabelsCommand, Result<Unit>>
{
    private readonly ILogger<SaveFaceLabelsCommandHandler> _logger;
    private readonly IVectorStore _vectorStore; // Changed from IVectorStoreService

    public SaveFaceLabelsCommandHandler(
        ILogger<SaveFaceLabelsCommandHandler> logger,
        IVectorStore vectorStore) // Changed constructor parameter
    {
        _logger = logger;
        _vectorStore = vectorStore; // Changed assignment
    }

    public async Task<Result<Unit>> Handle(SaveFaceLabelsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SaveFaceLabelsCommand for ImageId {ImageId} with {FaceCount} faces.",
            request.ImageId, request.FaceLabels.Count);

        foreach (var faceLabel in request.FaceLabels)
        {
            if (faceLabel.Embedding == null || !faceLabel.Embedding.Any())
            {
                _logger.LogWarning("Face {FaceId} has no embedding. Skipping vector storage.", faceLabel.Id);
                continue;
            }

            var embedding = faceLabel.Embedding;

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
            var textChunk = new TextChunk { Embedding = embedding.ToArray(), Metadata = metadata }; // Created TextChunk
            await _vectorStore.UpsertAsync(textChunk, cancellationToken); // Changed to UpsertAsync
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
