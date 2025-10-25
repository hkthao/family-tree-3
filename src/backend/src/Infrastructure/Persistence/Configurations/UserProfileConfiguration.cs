using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.Property(up => up.ExternalId)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(up => up.ExternalId)
            .IsUnique();

        builder.Property(up => up.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(up => up.Name)
            .HasMaxLength(256)
            .IsRequired();

        // Configure the one-to-many relationship with FamilyUser
        builder.HasMany(up => up.FamilyUsers)
            .WithOne(fu => fu.UserProfile)
            .HasForeignKey(fu => fu.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade); // If a user profile is deleted, their family associations are also deleted
    }
}
