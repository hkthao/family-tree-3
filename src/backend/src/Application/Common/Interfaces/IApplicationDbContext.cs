using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Family> Families { get; }
    DbSet<Member> Members { get; }
    DbSet<Event> Events { get; }
    DbSet<Relationship> Relationships { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<FamilyUser> FamilyUsers { get; }
    DbSet<UserActivity> UserActivities { get; }
    DbSet<UserPreference> UserPreferences { get; }
    DbSet<FileMetadata> FileMetadata { get; }
    DbSet<SystemConfiguration> SystemConfigurations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
