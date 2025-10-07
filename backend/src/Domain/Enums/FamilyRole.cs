namespace backend.Domain.Enums;

/// <summary>
/// Represents the role a user has within a family.
/// </summary>
public enum FamilyRole
{
    /// <summary>
    /// User has full management rights over the family.
    /// </summary>
    Manager = 0,

    /// <summary>
    /// User can view family data but cannot modify it.
    /// </summary>
    Viewer = 1,
}
