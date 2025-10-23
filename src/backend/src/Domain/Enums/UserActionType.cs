namespace backend.Domain.Enums;

/// <summary>
/// Defines the types of actions a user can perform.
/// </summary>
public enum UserActionType
{
    Login = 0,
    Logout = 1,
    CreateFamily = 2,
    UpdateFamily = 3,
    DeleteFamily = 4,
    CreateMember = 5,
    UpdateMember = 6,
    DeleteMember = 7,
    CreateEvent = 8,
    UpdateEvent = 9,
    DeleteEvent = 10,
    CreateRelationship = 11,
    UpdateRelationship = 12,
    DeleteRelationship = 13,
    ChangeRole = 14, // e.g., changing a user's role within a family
    // Add more action types as needed
}
