using System.Text.Json; // New using statement
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // New using statement

namespace backend.Infrastructure.Persistence.Configurations;

public class RelationConfiguration : IEntityTypeConfiguration<Relation>
{
    public void Configure(EntityTypeBuilder<Relation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired();

        // Configure ValueConverter for NamesByRegion
        var namesByRegionConverter = new ValueConverter<NamesByRegion, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<NamesByRegion>(v, (JsonSerializerOptions?)null)!
        );

        builder.Property(r => r.NamesByRegion)
            .HasConversion(namesByRegionConverter)
            .HasColumnType("json") // Keep HasColumnType for MySQL, it will be ignored by InMemory
            .IsRequired();
    }
}
