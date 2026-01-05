using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyLocationConfiguration : IEntityTypeConfiguration<FamilyLocation>
{
    public void Configure(EntityTypeBuilder<FamilyLocation> builder)
    {
        builder.HasOne(fl => fl.Family)
            .WithMany() // Assuming Family can have many FamilyLocations, but FamilyLocation points to one Family
            .HasForeignKey(fl => fl.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationship with Location
        builder.HasOne(fl => fl.Location)
            .WithMany()
            .HasForeignKey(fl => fl.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
