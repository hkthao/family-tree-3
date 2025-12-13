using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyMediaConfiguration : IEntityTypeConfiguration<FamilyMedia>
{
    public void Configure(EntityTypeBuilder<FamilyMedia> builder)
    {
        builder.HasOne(fm => fm.Family)
            .WithMany()
            .HasForeignKey(fm => fm.FamilyId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of Family if media exists
    }
}
