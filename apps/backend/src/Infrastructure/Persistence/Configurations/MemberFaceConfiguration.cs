using System.Text.Json; // Required for JsonSerializer
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemberFaceConfiguration : IEntityTypeConfiguration<MemberFace>
{
    public void Configure(EntityTypeBuilder<MemberFace> builder)
    {
        builder.OwnsOne(mf => mf.BoundingBox);
        // Configure Embedding to be stored as JSON
        builder.Property(mf => mf.Embedding)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<double>>(v, (JsonSerializerOptions?)null) ?? new List<double>()
            );
    }
}
