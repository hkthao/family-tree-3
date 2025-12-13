using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyLinkConfiguration : IEntityTypeConfiguration<FamilyLink>
{
    public void Configure(EntityTypeBuilder<FamilyLink> builder)
    {
        builder.HasOne(fl => fl.Family1)
            .WithMany()
            .HasForeignKey(fl => fl.Family1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fl => fl.Family2)
            .WithMany()
            .HasForeignKey(fl => fl.Family2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fl => new { fl.Family1Id, fl.Family2Id }).IsUnique();
    }
}
