namespace backend.Application.Families.ExportImport;

public class FamilyExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = null!;
    public List<MemberExportDto> Members { get; set; } = [];
    public List<RelationshipExportDto> Relationships { get; set; } = [];
    public List<EventExportDto> Events { get; set; } = [];
}
