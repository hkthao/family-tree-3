using System.Text.Json; // Required for JsonSerializer
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemberFaceConfiguration : IEntityTypeConfiguration<MemberFace>
{
    public void Configure(EntityTypeBuilder<MemberFace> builder)
    {
        builder.Property(mf => mf.BoundingBox)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<BoundingBox>(v, (JsonSerializerOptions?)null) ?? new BoundingBox()
            )
            .Metadata.SetValueComparer(new ValueComparer<BoundingBox>(
                (c1, c2) => c1!.Equals(c2!), // Assuming BoundingBox has a proper Equals implementation
                c => c.GetHashCode(),
                c => new BoundingBox { X = c.X, Y = c.Y, Width = c.Width, Height = c.Height })); // Use record's cloning behavior

        // Configure Embedding to be stored as JSON
        builder.Property(mf => mf.Embedding)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<double>>(v, (JsonSerializerOptions?)null) ?? new List<double>()
            )
            .Metadata.SetValueComparer(new ValueComparer<List<double>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));
    }
}
