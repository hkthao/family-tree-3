using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.ToTable("file_metadata");

        builder.Property(fm => fm.Id).HasColumnName("id");
        builder.Property(fm => fm.Created).HasColumnName("created");
        builder.Property(fm => fm.CreatedBy).HasColumnName("created_by");
        builder.Property(fm => fm.LastModified).HasColumnName("last_modified");
        builder.Property(fm => fm.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(fm => fm.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(fm => fm.Url)
            .HasColumnName("url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(fm => fm.StorageProvider)
            .HasColumnName("storage_provider")
            .IsRequired();

        builder.Property(fm => fm.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fm => fm.FileSize)
            .HasColumnName("file_size")
            .IsRequired();

        builder.Property(fm => fm.UploadedBy)
            .HasColumnName("uploaded_by")
            .HasMaxLength(36) // GUID string length
            .IsRequired();

        builder.Property(fm => fm.UsedByEntity)
            .HasColumnName("used_by_entity")
            .HasMaxLength(100);

        builder.Property(fm => fm.UsedById)
            .HasColumnName("used_by_id");

        builder.Property(fm => fm.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }
}
