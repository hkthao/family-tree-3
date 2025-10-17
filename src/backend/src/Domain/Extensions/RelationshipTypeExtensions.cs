using backend.Domain.Enums;

namespace backend.Domain.Extensions;

public static class RelationshipTypeExtensions
{
    public static RelationshipType GetInverseRelationshipType(this RelationshipType type)
    {
        return type switch
        {
            RelationshipType.Father => RelationshipType.Child,
            RelationshipType.Mother => RelationshipType.Child,
            RelationshipType.Child => RelationshipType.Parent,
            RelationshipType.Wife => RelationshipType.Husband,
            RelationshipType.Husband => RelationshipType.Wife,
            RelationshipType.Spouse => RelationshipType.Spouse, // Inverse of Spouse is Spouse
            RelationshipType.Sibling => RelationshipType.Sibling,
            RelationshipType.Grandfather => RelationshipType.Grandchild,
            RelationshipType.Grandmother => RelationshipType.Grandchild,
            RelationshipType.Grandchild => RelationshipType.Grandfather, // Assuming Grandfather as generic grandparent
            RelationshipType.Uncle => RelationshipType.NephewNiece,
            RelationshipType.Aunt => RelationshipType.NephewNiece,
            RelationshipType.NephewNiece => RelationshipType.Uncle, // Assuming Uncle as generic uncle/aunt
            RelationshipType.Cousin => RelationshipType.Cousin,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"No inverse relationship defined for {type}")
        };
    }
}
