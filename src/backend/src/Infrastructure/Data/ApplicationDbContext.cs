using System.Reflection;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<FamilyUser> FamilyUsers => Set<FamilyUser>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<FileMetadata> FileMetadata { get; set; } = null!;
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<UserConfig> UserConfigs => Set<UserConfig>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Ignore<JsonDocument>();

        base.OnModelCreating(builder);
    }
}
