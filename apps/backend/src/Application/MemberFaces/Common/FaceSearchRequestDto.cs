using System.Collections.Generic;

namespace backend.Application.MemberFaces.Common;

public class FaceSearchRequestDto
{
    public List<double> Embedding { get; set; } = null!;
    public string? FamilyId { get; set; }
    public string? MemberId { get; set; }
    public int TopK { get; set; } = 5;
}
