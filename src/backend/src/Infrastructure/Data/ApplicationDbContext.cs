using System.Reflection;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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

        builder.Entity<SystemConfiguration>()
            .HasIndex(sc => sc.Key)
            .IsUnique();

        builder.Entity<SystemConfiguration>()
            .Property(sc => sc.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Entity<SystemConfiguration>()
            .Property(sc => sc.Value)
            .IsRequired(false);

        builder.Entity<SystemConfiguration>()
            .Property(sc => sc.ValueType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Entity<UserConfig>()
            .HasKey(uc => new { uc.UserProfileId, uc.Key });

        builder.Entity<UserConfig>()
            .HasOne(uc => uc.UserProfile)
            .WithMany() // Assuming UserProfile has no direct collection of UserConfig
            .HasForeignKey(uc => uc.UserProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete if UserProfile is deleted

        builder.Entity<UserConfig>()
            .Property(uc => uc.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Entity<UserConfig>()
            .Property(uc => uc.Value)
            .IsRequired();

        builder.Entity<UserConfig>()
            .Property(uc => uc.ValueType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Entity<Family>()
            .Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Entity<Family>()
            .HasIndex(f => f.Code)
            .IsUnique();

        builder.Entity<Member>()
            .Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Entity<Member>()
            .HasIndex(m => m.Code)
            .IsUnique();

        builder.Entity<Event>()
            .Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Entity<Event>()
            .HasIndex(e => e.Code)
            .IsUnique();

        builder.Entity<Member>()
            .HasOne(m => m.Family)
            .WithMany()
            .HasForeignKey(m => m.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Relationship>()
            .HasOne(r => r.SourceMember)
            .WithMany(m => m.Relationships)
            .HasForeignKey(r => r.SourceMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Relationship>()
            .HasOne(r => r.TargetMember)
            .WithMany()
            .HasForeignKey(r => r.TargetMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .HasOne<Family>()
            .WithMany()
            .HasForeignKey(e => e.FamilyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure FamilyUser many-to-many relationship
        builder.Entity<FamilyUser>()
            .HasKey(fu => new { fu.FamilyId, fu.UserProfileId });

        builder.Entity<FamilyUser>()
            .HasOne(fu => fu.Family)
            .WithMany(f => f.FamilyUsers)
            .HasForeignKey(fu => fu.FamilyId);

        builder.Entity<FamilyUser>()
            .HasOne(fu => fu.UserProfile)
            .WithMany(up => up.FamilyUsers)
            .HasForeignKey(fu => fu.UserProfileId);

        // Configure UserPreference one-to-one relationship with UserProfile
        builder.Entity<UserPreference>()
            .HasKey(up => up.UserProfileId);

        builder.Entity<UserPreference>()
            .HasOne(up => up.UserProfile)
            .WithOne()
            .HasForeignKey<UserPreference>(up => up.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore<JsonDocument>();

        base.OnModelCreating(builder);
    }
}
