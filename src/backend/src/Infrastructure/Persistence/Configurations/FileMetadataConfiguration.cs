using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Domain.Extensions;

namespace backend.Infrastructure.Persistence.Configurations;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.ToTable("file_metadata");

        // Map all properties to kebab-case column names
        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name == "Id") continue; // Skip Id as it is handled separately
            property.SetColumnName(property.Name.ToKebabCase());
        }

        builder.Property(fm => fm.Id).HasColumnName("id");

        builder.Property(fm => fm.FileName)
            .HasMaxLength(255)
            .IsRequired()
            .HasColumnName("file_name");

        builder.Property(fm => fm.Url)
            .HasMaxLength(2048)
            .IsRequired()
            .HasColumnName("url");

        builder.Property(fm => fm.StorageProvider)
            .IsRequired()
            .HasColumnName("storage_provider");

        builder.Property(fm => fm.ContentType)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnName("content_type");

        builder.Property(fm => fm.FileSize)
            .IsRequired()
            .HasColumnName("file_size");

        builder.Property(fm => fm.UploadedBy)
            .HasMaxLength(36) // GUID string length
            .IsRequired()
            .HasColumnName("uploaded_by");

        builder.Property(fm => fm.UsedByEntity)
            .HasMaxLength(100)
            .HasColumnName("used_by_entity");

        builder.Property(fm => fm.IsActive)
            .IsRequired()
            .HasColumnName("is_active");
    }
}
