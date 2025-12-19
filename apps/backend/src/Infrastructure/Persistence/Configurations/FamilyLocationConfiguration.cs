using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyLocationConfiguration : IEntityTypeConfiguration<FamilyLocation>
{
    public void Configure(EntityTypeBuilder<FamilyLocation> builder)
    {
        builder.Property(fl => fl.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fl => fl.Description)
            .HasMaxLength(1000);

        builder.Property(fl => fl.Address)
            .HasMaxLength(500);

        builder.HasOne(fl => fl.Family)
            .WithMany() // Assuming Family can have many FamilyLocations, but FamilyLocation points to one Family
            .HasForeignKey(fl => fl.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Configure enums
        builder.Property(fl => fl.LocationType)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fl => fl.Accuracy)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fl => fl.Source)
            .HasConversion<string>() // Store enum as string
            .IsRequired()
            .HasMaxLength(50);
    }
}
