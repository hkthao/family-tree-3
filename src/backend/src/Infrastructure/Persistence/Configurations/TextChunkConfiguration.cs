using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class TextChunkConfiguration : IEntityTypeConfiguration<TextChunk>
{
    public void Configure(EntityTypeBuilder<TextChunk> builder)
    {
        builder.ToTable("text_chunk");

        builder.Property(t => t.Content)
            .HasColumnName("content")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(t => t.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(t => t.Category)
            .HasColumnName("category")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Source)
            .HasColumnName("source")
            .HasMaxLength(500);

        builder.Property(t => t.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        builder.Property(t => t.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("json");

        builder.Property(t => t.Score)
            .HasColumnName("score")
            .IsRequired();
    }
}
