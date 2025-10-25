using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        builder.ToTable("relationship");

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Created).HasColumnName("created");
        builder.Property(r => r.CreatedBy).HasColumnName("created_by");
        builder.Property(r => r.LastModified).HasColumnName("last_modified");
        builder.Property(r => r.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(r => r.SourceMemberId)
            .HasColumnName("source_member_id")
            .IsRequired();

        builder.Property(r => r.TargetMemberId)
            .HasColumnName("target_member_id")
            .IsRequired();

        builder.Property(r => r.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(r => r.Order)
            .HasColumnName("order");

        builder.Property(r => r.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.HasOne(r => r.SourceMember)
            .WithMany(m => m.Relationships)
            .HasForeignKey(r => r.SourceMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.TargetMember)
            .WithMany()
            .HasForeignKey(r => r.TargetMemberId)
            .OnDelete(DeleteBehavior.Restrict);    }
}
