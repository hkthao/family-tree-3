using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Queries.GetDetectedFaces;
public class GetDetectedFacesQueryHandler(ILogger<GetDetectedFacesQueryHandler> logger /*, IVectorStoreService vectorStoreService*/) : IRequestHandler<GetDetectedFacesQuery, Result<List<DetectedFaceDto>>>
{
    private readonly ILogger<GetDetectedFacesQueryHandler> _logger = logger;

    public async Task<Result<List<DetectedFaceDto>>> Handle(GetDetectedFacesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving detected faces for ImageId {ImageId} (placeholder).", request.ImageId);

        // TODO: Implement logic to retrieve detected faces from the vector store.
        // This would involve querying the vector store for faces associated with the ImageId
        // and mapping them to DetectedFaceDto.

        await Task.CompletedTask;

        return Result<List<DetectedFaceDto>>.Success([]);
    }
}
