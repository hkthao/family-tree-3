using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTree.Infrastructure.Persistence.Configurations;

public class LocationLinkConfiguration : IEntityTypeConfiguration<LocationLink>
{
    public void Configure(EntityTypeBuilder<LocationLink> builder)
    {
        builder.Property(t => t.RefId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.RefType)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000)
            .IsRequired();

        // Configure LinkType enum
        builder.Property(t => t.LinkType)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);

        // Configure relationship with Location
        builder.HasOne(ll => ll.Location)
            .WithMany() // No direct navigation property on Location back to LocationLink (loose coupling)
            .HasForeignKey(ll => ll.LocationId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete if Location has links
    }
}
