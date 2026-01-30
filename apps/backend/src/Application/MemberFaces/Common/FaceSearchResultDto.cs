namespace backend.Application.MemberFaces.Common;

public class FaceApiSearchResultDto
{
    public string Id { get; set; } = null!;
    public float Score { get; set; }
    public FaceSearchResultPayloadDto Payload { get; set; } = null!;
}
