using System;
using System.Collections.Generic;

namespace backend.Application.Knowledge.DTOs;

public class FaceSearchResultDto
{
    public Guid FaceId { get; set; }
    public Guid MemberId { get; set; }
    public double Score { get; set; }
    // Có thể thêm metadata nếu cần thiết
    // public MemberFaceDto? Metadata { get; set; } 
}
