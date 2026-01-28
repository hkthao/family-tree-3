using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.IntegrationEvents;

public class FaceAddIntegrationEvent
{
    public FaceAddVectorRequestDto FaceAddRequest { get; set; } = null!;
    public Guid MemberFaceLocalId { get; set; } // Local ID of the MemberFace entity in the backend
}
