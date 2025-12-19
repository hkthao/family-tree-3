using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemoryItemConfiguration : IEntityTypeConfiguration<MemoryItem>
{
    public void Configure(EntityTypeBuilder<MemoryItem> builder)
    {
        builder.ToTable("memory_items");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.FamilyId)
            .IsRequired();

        builder.Property(mi => mi.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(mi => mi.Description)
            .HasColumnType("text");

        builder.Property(mi => mi.HappenedAt);

        builder.Property(mi => mi.EmotionalTag)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50); // Adjust max length as needed for enum string representation

        // Configure soft delete
        builder.HasQueryFilter(mi => !mi.IsDeleted); // Exclude soft-deleted entities by default

        builder.Property(mi => mi.IsDeleted)
            .IsRequired();
        builder.Property(mi => mi.DeletedBy);
        builder.Property(mi => mi.DeletedDate);

        // Relationships
        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(mi => mi.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete on Family
    }
}
