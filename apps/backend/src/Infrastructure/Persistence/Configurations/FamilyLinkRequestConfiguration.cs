using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyLinkRequestConfiguration : IEntityTypeConfiguration<FamilyLinkRequest>
{
    public void Configure(EntityTypeBuilder<FamilyLinkRequest> builder)
    {
        builder.HasOne(flr => flr.RequestingFamily)
            .WithMany()
            .HasForeignKey(flr => flr.RequestingFamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(flr => flr.TargetFamily)
            .WithMany()
            .HasForeignKey(flr => flr.TargetFamilyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
