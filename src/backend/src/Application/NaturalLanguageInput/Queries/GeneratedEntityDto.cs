using backend.Application.Events;
using backend.Application.Families;
using backend.Application.Members.Queries;

namespace backend.Application.NaturalLanguageInput.Queries;

public class GeneratedEntityDto
{
    public string DataType { get; set; } = "Unknown"; // e.g., "Families", "Members", "Mixed"
    public List<FamilyDto> Families { get; set; } = [];
    public List<MemberDto> Members { get; set; } = [];
    public List<EventDto> Events { get; set; } = [];
}
