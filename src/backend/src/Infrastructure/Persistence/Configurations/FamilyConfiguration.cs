using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(f => f.Code)
            .IsUnique();
    }
}