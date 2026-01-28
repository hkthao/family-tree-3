using System.Collections.Generic;

namespace backend.Application.MemberFaces.Common;

public class FaceSearchResultDto
{
    public string Id { get; set; } = null!;
    public float Score { get; set; }
    public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();
}
