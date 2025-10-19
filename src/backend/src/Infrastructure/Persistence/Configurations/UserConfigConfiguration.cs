using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserConfigConfiguration : IEntityTypeConfiguration<UserConfig>
{
    public void Configure(EntityTypeBuilder<UserConfig> builder)
    {
        builder.HasKey(uc => new { uc.UserProfileId, uc.Key });

        builder.HasOne(uc => uc.UserProfile)
            .WithMany() // Assuming UserProfile has no direct collection of UserConfig
            .HasForeignKey(uc => uc.UserProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete if UserProfile is deleted

        builder.Property(uc => uc.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(uc => uc.Value)
            .IsRequired();

        builder.Property(uc => uc.ValueType)
            .IsRequired()
            .HasMaxLength(50);
    }
}