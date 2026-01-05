using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class LocationLink : BaseAuditableEntity
{
    // Private constructor to enforce creation via factory method
    private LocationLink() { }

    public string RefId { get; private set; } = null!;
    public RefType RefType { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid FamilyLocationId { get; private set; }

    // Navigation Property
    public FamilyLocation FamilyLocation { get; private set; } = null!;

    // Factory method for creation
    public static LocationLink Create(string refId, RefType refType, string description, Guid familyLocationId)
    {
        // Add any domain validation/business rules here
        // For example: if (string.IsNullOrWhiteSpace(refId)) throw new ArgumentException("RefId cannot be empty.");

        return new LocationLink
        {
            RefId = refId,
            RefType = refType,
            Description = description,
            FamilyLocationId = familyLocationId
        };
    }

    // Method for updating properties
    public void Update(string refId, RefType refType, string description, Guid familyLocationId)
    {
        // Add any domain validation/business rules here before updating
        RefId = refId;
        RefType = refType;
        Description = description;
        FamilyLocationId = familyLocationId;
    }
}
