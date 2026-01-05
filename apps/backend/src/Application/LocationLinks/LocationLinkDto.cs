using backend.Domain.Enums;

namespace backend.Application.LocationLinks;

public class LocationLinkDto
{
    public Guid Id { get; set; }
    public string RefId { get; set; } = null!;
    public RefType RefType { get; set; }
    public string Description { get; set; } = null!;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = null!; // To display linked location name
    public LocationLinkType LinkType { get; set; }
}
