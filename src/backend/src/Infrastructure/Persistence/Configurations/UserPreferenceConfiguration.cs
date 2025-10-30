using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preference");

        builder.Property(up => up.Id).HasColumnName("id");
        builder.Property(up => up.Created).HasColumnName("created");
        builder.Property(up => up.CreatedBy).HasColumnName("created_by");
        builder.Property(up => up.LastModified).HasColumnName("last_modified");
        builder.Property(up => up.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(up => up.UserProfileId).HasColumnName("user_profile_id");

        builder.Property(up => up.Theme)
            .HasColumnName("theme")
            .IsRequired();

        builder.Property(up => up.Language)
            .HasColumnName("language")
            .IsRequired();

        builder.HasKey(up => up.UserProfileId);
        
        builder.HasOne(up => up.UserProfile)
            .WithOne()
            .HasForeignKey<UserPreference>(up => up.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);    }
}
