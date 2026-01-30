namespace backend.Application.MemberFaces.Common;

public class FaceSearchVectorRequestDto
{
    public List<float> Embedding { get; set; } = null!;
    public string? FamilyId { get; set; }
    public string? MemberId { get; set; }
    public int TopK { get; set; } = 5;
    public float Threshold { get; set; } = 0.7f;
}
