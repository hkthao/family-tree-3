using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Message)
            .HasColumnName("message")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.RecipientUserId)
            .HasColumnName("recipient_user_id")
            .IsRequired();

        builder.Property(t => t.SenderUserId)
            .HasColumnName("sender_user_id")
            .HasMaxLength(36);

        builder.Property(t => t.ReadAt)
            .HasColumnName("read_at");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(t => t.FamilyId)
            .HasColumnName("family_id");
    }
}
