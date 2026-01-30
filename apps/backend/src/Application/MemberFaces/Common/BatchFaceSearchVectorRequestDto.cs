namespace backend.Application.MemberFaces.Common;

public class BatchFaceSearchVectorRequestDto
{
    public List<List<float>> Vectors { get; set; } = new List<List<float>>();
    public Guid FamilyId { get; set; }
    public int Limit { get; set; } = 1;
    public float Threshold { get; set; } = 0.7f;
}
