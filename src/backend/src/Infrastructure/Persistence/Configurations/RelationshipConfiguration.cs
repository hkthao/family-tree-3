using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Domain.Extensions;

namespace backend.Infrastructure.Persistence.Configurations;

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        builder.ToTable("relationships");

        // Map all properties to kebab-case column names
        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name == "Id" || property.Name == "SourceMemberId" || property.Name == "TargetMemberId") continue; // Skip Id and FKs as they are handled separately
            property.SetColumnName(property.Name.ToKebabCase());
        }

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.SourceMemberId).HasColumnName("source_member_id");
        builder.Property(r => r.TargetMemberId).HasColumnName("target_member_id");

        builder.HasOne(r => r.SourceMember)
            .WithMany(m => m.Relationships)
            .HasForeignKey(r => r.SourceMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.TargetMember)
            .WithMany()
            .HasForeignKey(r => r.TargetMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
