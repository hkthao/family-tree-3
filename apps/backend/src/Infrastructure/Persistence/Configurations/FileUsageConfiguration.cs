using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FileUsageConfiguration : IEntityTypeConfiguration<FileUsage>
{
    public void Configure(EntityTypeBuilder<FileUsage> builder)
    {
        builder.ToTable("file_usage");

        builder.HasKey(fu => new { fu.FileMetadataId, fu.EntityType, fu.EntityId });

        builder.Property(fu => fu.FileMetadataId);
        builder.Property(fu => fu.EntityType)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(fu => fu.EntityId);

        builder.HasOne(fu => fu.FileMetadata)
            .WithMany(fm => fm.FileUsages)
            .HasForeignKey(fu => fu.FileMetadataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
