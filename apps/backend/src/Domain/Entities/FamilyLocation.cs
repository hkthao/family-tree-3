using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class FamilyLocation : BaseAuditableEntity, IAggregateRoot
{
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public LocationType LocationType { get; set; }
    public LocationAccuracy Accuracy { get; set; } = LocationAccuracy.Estimated;
    public LocationSource Source { get; set; } = LocationSource.UserSelected;
}
