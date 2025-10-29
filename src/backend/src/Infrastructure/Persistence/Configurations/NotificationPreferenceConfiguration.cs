using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preference");

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.NotificationType)
            .HasColumnName("notification_type")
            .IsRequired();

        builder.Property(t => t.Channel)
            .HasColumnName("channel")
            .IsRequired();

        builder.Property(t => t.Enabled)
            .HasColumnName("enabled")
            .IsRequired();

        builder.HasIndex(np => new { np.UserId, np.NotificationType, np.Channel })
            .HasDatabaseName("ix_notification_preference_user_id_notification_type_channel")
            .IsUnique();
    }
}
