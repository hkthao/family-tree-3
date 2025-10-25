using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Domain.Extensions;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preferences");

        // Map all properties to kebab-case column names
        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name == "UserProfileId") continue; // Skip UserProfileId as it is part of the key and FK
            property.SetColumnName(property.Name.ToKebabCase());
        }

        builder.HasKey(up => up.UserProfileId);
        builder.Property(up => up.UserProfileId).HasColumnName("user_profile_id");

        builder.HasOne(up => up.UserProfile)
            .WithOne()
            .HasForeignKey<UserPreference>(up => up.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
