using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Domain.Extensions;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        // Map all properties to kebab-case column names
        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name == "Id") continue; // Skip Id as it is handled separately
            property.SetColumnName(property.Name.ToKebabCase());
        }

        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.HasIndex(f => f.Code)
            .IsUnique();
    }
}
