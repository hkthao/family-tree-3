using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using backend.Application.AI.VectorStore; // Added
using Microsoft.Extensions.Options; // Added
using backend.Domain.Enums; // Added

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
                { "FaceId", faceLabel.Id },
                { "ImageId", request.ImageId.ToString() },
                { "MemberId", faceLabel.MemberId?.ToString() ?? string.Empty },
                { "MemberName", faceLabel.MemberName ?? string.Empty },
                { "FamilyId", faceLabel.FamilyId?.ToString() ?? string.Empty },
                { "FamilyName", faceLabel.FamilyName ?? string.Empty },
                { "BirthYear", faceLabel.BirthYear?.ToString() ?? string.Empty },
                { "DeathYear", faceLabel.DeathYear?.ToString() ?? string.Empty },
                { "BoundingBox_X", faceLabel.BoundingBox.X.ToString() },
                { "BoundingBox_Y", faceLabel.BoundingBox.Y.ToString() },
                { "BoundingBox_Width", faceLabel.BoundingBox.Width.ToString() },
                { "BoundingBox_Height", faceLabel.BoundingBox.Height.ToString() },
            };

            // Save embedding and metadata to vector store
            await vectorStore.UpsertAsync([.. embedding], metadata, cancellationToken);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
