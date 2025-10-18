using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using backend.Application.AI.VectorStore;
using Microsoft.Extensions.Options;
using backend.Domain.Enums;

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandler : IRequestHandler<SaveFaceLabelsCommand, Result<Unit>>
{
    private readonly ILogger<SaveFaceLabelsCommandHandler> _logger;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly VectorStoreSettings _vectorStoreSettings;

    public SaveFaceLabelsCommandHandler(
        ILogger<SaveFaceLabelsCommandHandler> logger,
        IVectorStoreFactory vectorStoreFactory,
        IOptions<VectorStoreSettings> vectorStoreSettingsOptions)
    {
        _logger = logger;
        _vectorStoreFactory = vectorStoreFactory;
        _vectorStoreSettings = vectorStoreSettingsOptions.Value;
    }

    public async Task<Result<Unit>> Handle(SaveFaceLabelsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SaveFaceLabelsCommand for ImageId {ImageId} with {FaceCount} faces.",
            request.ImageId, request.FaceLabels.Count);

        IVectorStore vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));
        var collectionName = "family-face-embeddings";
        var dim = 128;

        foreach (var faceLabel in request.FaceLabels)
        {
            if (faceLabel.Embedding == null || faceLabel.Embedding.Count == 0)
            {
                _logger.LogWarning("Face {FaceId} has no embedding. Skipping vector storage.", faceLabel.Id);
                continue;
            }

            var embedding = faceLabel.Embedding;

            // Prepare metadata for vector store
            var metadata = new Dictionary<string, string>
            {
                { "face_id", faceLabel.Id },
                { "image_id", request.ImageId.ToString() },
                { "member_id", faceLabel.MemberId?.ToString() ?? string.Empty },
                { "member_name", faceLabel.MemberName ?? string.Empty },
                { "family_id", faceLabel.FamilyId?.ToString() ?? string.Empty },
                { "family_name", faceLabel.FamilyName ?? string.Empty },
                { "birth_year", faceLabel.BirthYear?.ToString() ?? string.Empty },
                { "death_year", faceLabel.DeathYear?.ToString() ?? string.Empty },
                { "bounding_box_x", faceLabel.BoundingBox.X.ToString() },
                { "bounding_box_y", faceLabel.BoundingBox.Y.ToString() },
                { "bounding_box_hidth", faceLabel.BoundingBox.Width.ToString() },
                { "bounding_box_height", faceLabel.BoundingBox.Height.ToString() },
            };

            // Save embedding and metadata to vector store
            await vectorStore.UpsertAsync([.. embedding], metadata, collectionName, dim, cancellationToken);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
