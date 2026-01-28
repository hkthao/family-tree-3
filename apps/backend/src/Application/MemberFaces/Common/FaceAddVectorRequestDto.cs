namespace backend.Application.MemberFaces.Common;

public class FaceAddVectorRequestDto
{
    public List<double> Vector { get; set; } = null!;
    public FaceMetadataDto Metadata { get; set; } = null!;
}
