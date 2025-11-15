using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FaceConfiguration : IEntityTypeConfiguration<Face>
{
    public void Configure(EntityTypeBuilder<Face> builder)
    {
        builder.Property(f => f.MemberId)
            .IsRequired();

        builder.HasOne(f => f.Member)
            .WithMany(m => m.Faces)
            .HasForeignKey(f => f.MemberId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete faces when a member is deleted

        builder.Property(f => f.Thumbnail)
            .HasColumnType("longtext"); // Use longtext for potentially large thumbnails

        var listDoubleComparer = new ValueComparer<List<double>>(
            (l1, l2) => l1 != null && l2 != null && l1.SequenceEqual(l2),
            l => l.Aggregate(0, (hash, d) => HashCode.Combine(hash, d.GetHashCode())),
            l => l.ToList());

        var embeddingPropertyBuilder = builder.Property(f => f.Embedding);

        embeddingPropertyBuilder.HasConversion(
            v => string.Join(",", v ?? new List<double>()), // Convert List<double> to comma-separated string, handle null
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList() // Convert string back to List<double>
        );

        embeddingPropertyBuilder.Metadata.SetValueComparer(listDoubleComparer);
        embeddingPropertyBuilder.HasColumnType("longtext");
    }
}
