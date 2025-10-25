using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Domain.Extensions;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        // Map all properties to kebab-case column names
        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name == "Id" || property.Name == "FamilyId") continue; // Skip Id and FamilyId as they are handled separately
            property.SetColumnName(property.Name.ToKebabCase());
        }

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.FamilyId).HasColumnName("family_id");

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.HasIndex(m => m.Code)
            .IsUnique();

        builder.HasOne(m => m.Family)
            .WithMany()
            .HasForeignKey(m => m.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
