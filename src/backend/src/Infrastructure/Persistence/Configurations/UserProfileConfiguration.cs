using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profile");

        builder.Property(up => up.Id).HasColumnName("id");
        builder.Property(up => up.Created).HasColumnName("created");
        builder.Property(up => up.CreatedBy).HasColumnName("created_by");
        builder.Property(up => up.LastModified).HasColumnName("last_modified");
        builder.Property(up => up.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(up => up.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(up => up.ExternalId)
            .IsUnique();

        builder.Property(up => up.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(up => up.Name)
            .HasColumnName("name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(up => up.Avatar)
            .HasColumnName("avatar")
            .HasMaxLength(500);

        // Configure the one-to-many relationship with FamilyUser
        builder.HasMany(up => up.FamilyUsers)
            .WithOne(fu => fu.UserProfile)
            .HasForeignKey(fu => fu.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade); // If a user profile is deleted, their family associations are also deleted
    }
}
