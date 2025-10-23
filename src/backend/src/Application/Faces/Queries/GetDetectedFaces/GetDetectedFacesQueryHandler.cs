using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Queries.GetDetectedFaces;
public class GetDetectedFacesQueryHandler(ILogger<GetDetectedFacesQueryHandler> logger /*, IVectorStoreService vectorStoreService*/) : IRequestHandler<GetDetectedFacesQuery, List<DetectedFaceDto>>
{
    private readonly ILogger<GetDetectedFacesQueryHandler> _logger = logger;

    public async Task<List<DetectedFaceDto>> Handle(GetDetectedFacesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving detected faces for ImageId {ImageId} (placeholder).", request.ImageId);

        // TODO: Implement logic to retrieve detected faces from the vector store.
        // This would involve querying the vector store for faces associated with the ImageId
        // and mapping them to DetectedFaceDto.

        await Task.CompletedTask;

        return [];
    }
}
