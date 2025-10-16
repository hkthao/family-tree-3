namespace backend.Domain.Enums;

public enum RelationshipType
{
    Father,
    Mother,
    Wife,
    Husband,
    Child,
    Parent, // Generic parent, can be Father or Mother
    Sibling,
    Grandfather,
    Grandmother,
    Grandchild,
    Uncle,
    Aunt,
    NephewNiece, // Generic for Nephew or Niece
    Cousin,
    Spouse // Generic for Wife or Husband
}
