using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.HasKey(nd => nd.Id);

        builder.Property(nd => nd.EventId)
            .IsRequired();

        builder.Property(nd => nd.DeliveryDate)
            .IsRequired();

        builder.Property(nd => nd.DeliveryMethod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(nd => nd.IsSent)
            .IsRequired();

        builder.Property(nd => nd.SentAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(nd => nd.Event)
            .WithMany()
            .HasForeignKey(nd => nd.EventId)
            .OnDelete(DeleteBehavior.Cascade); // Adjust delete behavior as needed
    }
}
