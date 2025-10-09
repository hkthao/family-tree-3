using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Family> Families { get; }
    DbSet<Member> Members { get; }
    DbSet<Event> Events { get; }
    DbSet<Relationship> Relationships { get; }
    DbSet<UserProfile> UserProfiles { get; } // Added
    DbSet<FamilyUser> FamilyUsers { get; } // Added
    DbSet<UserActivity> UserActivities { get; }
    DbSet<AIBiography> AIBiographies { get; }
    DbSet<UserPreference> UserPreferences { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}