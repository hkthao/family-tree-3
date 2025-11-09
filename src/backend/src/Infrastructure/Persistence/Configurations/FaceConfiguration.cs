using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
            .HasMaxLength(2000); // Assuming thumbnail is a base64 string or URL

        builder.Property(f => f.Embedding)
            .HasConversion(
                v => string.Join(",", v ?? new List<double>()), // Convert List<double> to comma-separated string, handle null
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList() // Convert string back to List<double>
            )
            .HasColumnType("longtext"); // Use longtext for potentially large embeddings
    }
}
