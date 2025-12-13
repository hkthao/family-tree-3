using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MediaLinkConfiguration : IEntityTypeConfiguration<MediaLink>
{
    public void Configure(EntityTypeBuilder<MediaLink> builder)
    {
        builder.HasOne(ml => ml.FamilyMedia)
            .WithMany(fm => fm.MediaLinks)
            .HasForeignKey(ml => ml.FamilyMediaId)
            .OnDelete(DeleteBehavior.Cascade); // Delete MediaLink if FamilyMedia is deleted
    }
}
