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

        // Configure relationship with FamilyLocation
        builder.HasOne(ll => ll.FamilyLocation)
            .WithMany() // No direct navigation property on FamilyLocation back to LocationLink (loose coupling)
            .HasForeignKey(ll => ll.FamilyLocationId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete if FamilyLocation has links
    }
}
