using backend.Domain.Enums;

namespace backend.Domain.Extensions;

public static class RelationshipTypeExtensions
{
    public static string ToInverseDisplayType(this RelationshipType type)
    {
        return type switch
        {
            RelationshipType.Father => "Child",
            RelationshipType.Mother => "Child",
            RelationshipType.Husband => "Wife",
            RelationshipType.Wife => "Husband",
            _ => type.ToString() // Default to original type if no specific inverse display type
        };
    }
}
