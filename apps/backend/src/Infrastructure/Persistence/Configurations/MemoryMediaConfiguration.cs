using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemoryMediaConfiguration : IEntityTypeConfiguration<MemoryMedia>
{
    public void Configure(EntityTypeBuilder<MemoryMedia> builder)
    {
        builder.ToTable("memory_media");

        builder.HasKey(mm => mm.Id);

        builder.Property(mm => mm.MemoryItemId)
            .IsRequired();

        builder.Property(mm => mm.MediaType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50); // Adjust max length as needed

        builder.Property(mm => mm.Url)
            .HasMaxLength(1000)
            .IsRequired();

        // Relationships
        builder.HasOne(mm => mm.MemoryItem)
            .WithMany(mi => mi.MemoryMedia)
            .HasForeignKey(mm => mm.MemoryItemId)
            .OnDelete(DeleteBehavior.Cascade); // Media should be deleted if MemoryItem is deleted
    }
}
