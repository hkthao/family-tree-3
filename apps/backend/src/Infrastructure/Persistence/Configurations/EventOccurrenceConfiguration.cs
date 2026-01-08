using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class EventOccurrenceConfiguration : IEntityTypeConfiguration<EventOccurrence>
{
    public void Configure(EntityTypeBuilder<EventOccurrence> builder)
    {
        builder.HasKey(eo => eo.Id);

        builder.Property(eo => eo.EventId)
            .IsRequired();

        builder.Property(eo => eo.Year)
            .IsRequired();

        builder.Property(eo => eo.OccurrenceDate)
            .IsRequired();

        // Configure the relationship with the Event entity
        builder.HasOne(eo => eo.Event)
            .WithMany() // Assuming Event can have many EventOccurrences, but EventOccurrence does not necessarily have a collection navigation property to EventOccurrence
            .HasForeignKey(eo => eo.EventId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // If an Event is deleted, its occurrences should also be deleted
    }
}
