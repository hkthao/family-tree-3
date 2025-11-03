using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.Address)
            .HasMaxLength(500);

        builder.Property(f => f.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(f => f.Visibility)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(f => f.Code)
            .IsUnique();

        // Configure the private collections for FamilyUsers, Members, and Relationships
        builder.HasMany(f => f.FamilyUsers)
            .WithOne(fu => fu.Family)
            .HasForeignKey(fu => fu.FamilyId);

        builder.HasMany(f => f.Members)
            .WithOne(m => m.Family)
            .HasForeignKey(m => m.FamilyId);

        builder.HasMany(f => f.Relationships)
            .WithOne(r => r.Family)
            .HasForeignKey(r => r.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}