using Microsoft.Extensions.Logging;

namespace FamilyTree.Application.Faces.Queries.GetDetectedFaces;
public class GetDetectedFacesQueryHandler : IRequestHandler<GetDetectedFacesQuery, List<DetectedFaceDto>>
{
    private readonly ILogger<GetDetectedFacesQueryHandler> _logger;
    // private readonly IVectorStoreService _vectorStoreService; // Placeholder for vector store service

    public GetDetectedFacesQueryHandler(ILogger<GetDetectedFacesQueryHandler> logger /*, IVectorStoreService vectorStoreService*/)
    {
        _logger = logger;
        // _vectorStoreService = vectorStoreService;
    }

    public async Task<List<DetectedFaceDto>> Handle(GetDetectedFacesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving detected faces for ImageId {ImageId} (placeholder).", request.ImageId);

        // TODO: Implement logic to retrieve detected faces from the vector store.
        // This would involve querying the vector store for faces associated with the ImageId
        // and mapping them to DetectedFaceDto.

        await Task.CompletedTask;

        return new List<DetectedFaceDto>();
    }
}