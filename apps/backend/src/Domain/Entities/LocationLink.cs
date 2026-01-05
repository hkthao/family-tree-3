using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class LocationLink : BaseAuditableEntity
{
    // Private constructor to enforce creation via factory method
    private LocationLink() { }

    public string RefId { get; private set; } = null!;
    public RefType RefType { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid LocationId { get; private set; }
    public LocationLinkType LinkType { get; private set; } // NEW: LocationLinkType

    // Navigation Property
    public Location Location { get; private set; } = null!;

    // Factory method for creation
    public static LocationLink Create(string refId, RefType refType, string description, Guid locationId, LocationLinkType linkType)
    {
        // Add any domain validation/business rules here
        // For example: if (string.IsNullOrWhiteSpace(refId)) throw new ArgumentException("RefId cannot be empty.");

        return new LocationLink
        {
            RefId = refId,
            RefType = refType,
            Description = description,
            LocationId = locationId,
            LinkType = linkType // NEW: Assign LinkType
        };
    }

    // Method for updating properties
    public void Update(string refId, RefType refType, string description, Guid locationId, LocationLinkType linkType)
    {
        // Add any domain validation/business rules here before updating
        RefId = refId;
        RefType = refType;
        Description = description;
        LocationId = locationId;
        LinkType = linkType; // NEW: Update LinkType
    }
}
