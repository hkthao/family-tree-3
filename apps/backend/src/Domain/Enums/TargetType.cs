namespace backend.Domain.Enums;

/// <summary>
/// Defines the types of resources that can be targeted by a user action.
/// </summary>
public enum TargetType
{
    None = 0,
    Family = 1,
    Member = 2,
    UserProfile = 3,
    Event = 4,
    Relationship = 5,
}
