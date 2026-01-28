namespace backend.Application.MemberFaces.Common;

public class FaceSearchRequestDto
{
    public string QueryImage { get; set; } = null!; // base64 encoded image
    public string? FamilyId { get; set; }
    public int Limit { get; set; } = 5;
}
