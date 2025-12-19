using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemoryPersonConfiguration : IEntityTypeConfiguration<MemoryPerson>
{
    public void Configure(EntityTypeBuilder<MemoryPerson> builder)
    {
        builder.ToTable("memory_persons");

        builder.HasKey(mp => new { mp.MemoryItemId, mp.MemberId }); // Composite primary key

        builder.Property(mp => mp.MemoryItemId)
            .IsRequired();

        builder.Property(mp => mp.MemberId)
            .IsRequired();

        // Relationships
        builder.HasOne(mp => mp.MemoryItem)
            .WithMany(mi => mi.MemoryPersons)
            .HasForeignKey(mp => mp.MemoryItemId)
            .OnDelete(DeleteBehavior.Cascade); // Persons link should be deleted if MemoryItem is deleted

        builder.HasOne(mp => mp.Member)
            .WithMany() // Member can be associated with many MemoryPersons
            .HasForeignKey(mp => mp.MemberId)
            .OnDelete(DeleteBehavior.Restrict); // Do not delete Member if MemoryPerson is deleted
    }
}
