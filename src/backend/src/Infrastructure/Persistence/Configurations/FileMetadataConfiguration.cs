using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {

        builder.Property(fm => fm.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(fm => fm.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(fm => fm.StorageProvider)
            .IsRequired();

        builder.Property(fm => fm.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fm => fm.FileSize)
            .IsRequired();

        builder.Property(fm => fm.IsDeleted)
            .IsRequired();

        builder.Property(fm => fm.DeletedBy)
            .HasMaxLength(36); // GUID string length
    }
}
