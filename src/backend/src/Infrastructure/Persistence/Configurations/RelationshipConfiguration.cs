using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
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
