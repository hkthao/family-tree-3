using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.HasIndex(sc => sc.Key)
            .IsUnique();

        builder.Property(sc => sc.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sc => sc.Value)
            .IsRequired(false);

        builder.Property(sc => sc.ValueType)
            .IsRequired(false)
            .HasMaxLength(50);
    }
}