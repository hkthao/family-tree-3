using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Property(l => l.Address)
            .HasMaxLength(500);

        // Configure enums
        builder.Property(l => l.LocationType)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Accuracy)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Source)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);
    }
}
