using System.Reflection;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Family> Families => Set<Family>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Relationship> Relationships => Set<Relationship>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>(); // Added
        public DbSet<FamilyUser> FamilyUsers => Set<FamilyUser>(); // Added
        public DbSet<UserActivity> UserActivities => Set<UserActivity>();
        public DbSet<AIBiography> AIBiographies => Set<AIBiography>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<FileMetadata> FileMetadata { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<Member>()
                .HasOne<Family>()
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

            builder.Entity<AIBiography>()
                .Property(b => b.Metadata)
                .HasColumnType("json")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => v == null ? null : JsonDocument.Parse(v, new JsonDocumentOptions()));

            base.OnModelCreating(builder);
        }
    }
}
