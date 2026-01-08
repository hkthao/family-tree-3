using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserPushTokenConfiguration : IEntityTypeConfiguration<UserPushToken>
{
    public void Configure(EntityTypeBuilder<UserPushToken> builder)
    {
        builder.HasKey(upt => upt.Id);

        builder.Property(upt => upt.ExpoPushToken)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(upt => upt.Platform)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(upt => upt.DeviceId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(upt => upt.IsActive)
            .IsRequired();

        // Configure the one-to-many relationship with User
        builder.HasOne(upt => upt.User)
            .WithMany(u => u.UserPushTokens)
            .HasForeignKey(upt => upt.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, their push tokens are also deleted
    }
}
