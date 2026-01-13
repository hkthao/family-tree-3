namespace backend.Application.Families.Commands.Import;

public class FamilyImportDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FamilyHistory { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Visibility { get; set; } = "Private";
}
