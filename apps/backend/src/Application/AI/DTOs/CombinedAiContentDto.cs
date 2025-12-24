using backend.Application.Events.Queries;
using backend.Application.Families.Queries;
using backend.Application.Members.Queries;

namespace backend.Application.AI.DTOs;

/// <summary>
/// DTO chứa nội dung AI tổng hợp từ nhiều loại đối tượng.
/// </summary>
public class CombinedAiContentDto
{
    public List<FamilyDto>? Families { get; set; }
    public List<MemberDto>? Members { get; set; }
    public List<EventDto>? Events { get; set; }
}
