namespace backend.Application.Families.Queries.GetFamilyDetails;

public class MemberDetailsDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; } // Formatted date string
    public int EventMembersCount { get; set; }
}

public class FamilyDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<MemberDetailsDto> Members { get; set; } = new List<MemberDetailsDto>();
}
