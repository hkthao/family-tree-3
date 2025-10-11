using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Location)
            .HasMaxLength(200);

        builder.Property(t => t.Color)
            .HasMaxLength(20);

        builder
            .HasMany(e => e.RelatedMembers)
            .WithMany(); // This configures the many-to-many relationship
    }
}
