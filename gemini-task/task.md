Refactor the current IUser interface and user identity handling system.

Requirements:
- Current IUser.Id returns external provider ID (e.g., Google/Firebase).
- Introduce separation between ExternalId and ProfileId.
- ProfileId (Guid) maps to UserProfile table in the internal database.
- Update IUser interface to include ExternalId, ProfileId, Email, and DisplayName.
- Implement CurrentUserService that resolves ProfileId based on ExternalId using AppDbContext.
- All audit fields (CreatedBy, UpdatedBy) in domain entities must use ProfileId.
- Ensure compatibility with existing external authentication provider.
