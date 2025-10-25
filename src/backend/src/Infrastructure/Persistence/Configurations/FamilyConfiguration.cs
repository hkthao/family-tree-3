using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("family");

        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.Created).HasColumnName("created");
        builder.Property(f => f.CreatedBy).HasColumnName("created_by");
        builder.Property(f => f.LastModified).HasColumnName("last_modified");
        builder.Property(f => f.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(f => f.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(f => f.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(f => f.Visibility)
            .HasColumnName("visibility")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.TotalMembers)
            .HasColumnName("total_members")
            .IsRequired();

        builder.Property(f => f.TotalGenerations)
            .HasColumnName("total_generations")
            .IsRequired();

        builder.HasIndex(f => f.Code)
            .IsUnique();
    }
}
