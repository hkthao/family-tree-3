using Microsoft.Extensions.Logging;

namespace FamilyTree.Application.Faces.Commands.LabelFace;
public class LabelFaceCommandHandler : IRequestHandler<LabelFaceCommand, Unit>
{
    private readonly ILogger<LabelFaceCommandHandler> _logger;
    // private readonly IVectorStoreService _vectorStoreService; // Placeholder for vector store service

    public LabelFaceCommandHandler(ILogger<LabelFaceCommandHandler> logger /*, IVectorStoreService vectorStoreService*/)
    {
        _logger = logger;
        // _vectorStoreService = vectorStoreService;
    }

    public async Task<Unit> Handle(LabelFaceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Labeling face {FaceId} for member {MemberId}.", request.FaceId, request.MemberId);

        // TODO: Implement logic to store face metadata and embedding in the vector store.
        // This would involve:
        // 1. Generating an embedding for the face (if not already done by Python service).
        // 2. Storing the embedding along with metadata (MemberId, BoundingBox, Confidence, Thumbnail) in the vector store.

        _logger.LogInformation("Face {FaceId} labeled and stored in vector store (placeholder).", request.FaceId);

        await Task.CompletedTask;

        return Unit.Value;
    }
}