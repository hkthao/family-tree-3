using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {

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

        builder.Property(up => up.FirstName)
            .HasMaxLength(256);

        builder.Property(up => up.LastName)
            .HasMaxLength(256);

        builder.Property(up => up.Phone)
            .HasMaxLength(256);

        builder.Property(up => up.Avatar)
            .HasMaxLength(500);

        builder.Property(up => up.UserId)
            .IsRequired();

        builder.HasOne(up => up.User)
            .WithOne(u => u.Profile) // Specify the navigation property in User
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade); // If User is deleted, delete UserProfile

    }
}
