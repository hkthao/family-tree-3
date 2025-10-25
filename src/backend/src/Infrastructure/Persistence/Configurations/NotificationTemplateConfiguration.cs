using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.HasKey(nt => nt.Id);

        builder.Property(nt => nt.EventType)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("event_type");

        builder.Property(nt => nt.Channel)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("channel");

        builder.Property(nt => nt.Subject)
            .HasMaxLength(250)
            .IsRequired()
            .HasColumnName("subject");

        builder.Property(nt => nt.Body)
            .HasColumnType("longtext")
            .IsRequired()
            .HasColumnName("body");

        builder.Property(nt => nt.Format)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("format");

        builder.Property(nt => nt.LanguageCode)
            .HasMaxLength(10)
            .IsRequired()
            .HasColumnName("language_code");

        builder.Property(nt => nt.Placeholders)
            .HasColumnType("json")
            .IsRequired(false) // Placeholders can be null
            .HasColumnName("placeholders");

        builder.Property(nt => nt.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        // BaseAuditableEntity properties
        builder.Property(nt => nt.Created)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(nt => nt.CreatedBy)
            .HasMaxLength(256)
            .HasColumnName("created_by");

        builder.Property(nt => nt.LastModified)
            .HasColumnName("last_modified_at");

        builder.Property(nt => nt.LastModifiedBy)
            .HasMaxLength(256)
            .HasColumnName("last_modified_by");
    }
}
