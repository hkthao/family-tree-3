namespace backend.Domain.Entities;

public class FamilyLocation : BaseAuditableEntity, IAggregateRoot
{
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;
    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;

    // Private constructor for EF Core and other internal uses
    private FamilyLocation()
    {
        Family = null!; // For EF Core
        Location = null!; // For EF Core
    }

    public FamilyLocation(Guid familyId, Guid locationId)
    {
        FamilyId = familyId;
        LocationId = locationId;
    }
}
