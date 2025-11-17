using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class PrivacyConfigurationConfiguration : IEntityTypeConfiguration<PrivacyConfiguration>
{
    public void Configure(EntityTypeBuilder<PrivacyConfiguration> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.PublicMemberProperties)
            .HasMaxLength(2000) // Adjust max length as needed
            .IsRequired();

        builder.HasOne(pc => pc.Family)
            .WithOne(f => f.PrivacyConfiguration)
            .HasForeignKey<PrivacyConfiguration>(pc => pc.FamilyId)
            .OnDelete(DeleteBehavior.Cascade); // If family is deleted, delete its privacy config
    }
}
