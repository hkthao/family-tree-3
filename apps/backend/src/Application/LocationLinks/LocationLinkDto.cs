using backend.Domain.Enums;

namespace backend.Application.LocationLinks;

public class LocationLinkDto
{
    public int Id { get; set; }
    public string RefId { get; set; } = null!;
    public RefType RefType { get; set; }
    public string Description { get; set; } = null!;
    public int FamilyLocationId { get; set; }
    public string FamilyLocationName { get; set; } = null!; // To display linked location name
}
