using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Domain.Extensions;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        // Map all properties to kebab-case column names
        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name == "Id") continue; // Skip Id as it is handled separately
            property.SetColumnName(property.Name.ToKebabCase());
        }

        builder.Property(up => up.Id).HasColumnName("id");

        builder.Property(up => up.ExternalId)
            .HasMaxLength(256)
            .IsRequired()
            .HasColumnName("external_id");

        builder.HasIndex(up => up.ExternalId)
            .IsUnique();

        builder.Property(up => up.Email)
            .HasMaxLength(256)
            .IsRequired()
            .HasColumnName("email");

        builder.Property(up => up.Name)
            .HasMaxLength(256)
            .IsRequired()
            .HasColumnName("name");

        // Configure the one-to-many relationship with FamilyUser
        builder.HasMany(up => up.FamilyUsers)
            .WithOne(fu => fu.UserProfile)
            .HasForeignKey(fu => fu.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade); // If a user profile is deleted, their family associations are also deleted
    }
}
