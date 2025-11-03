using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        builder.Property(r => r.SourceMemberId)
            .IsRequired();

        builder.Property(r => r.TargetMemberId)
            .IsRequired();

        builder.Property(r => r.Type)
            .IsRequired();

        builder.Property(r => r.FamilyId)
            .IsRequired();

        builder.HasOne(r => r.Family)
            .WithMany(f => f.Relationships) // Referencing the public property
            .HasForeignKey(r => r.FamilyId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete relationships when family is deleted

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
