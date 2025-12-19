using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // Explicitly set the table name to avoid potential conflicts
        builder.ToTable("Events");

        // Explicitly define Id as the primary key of Event
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired();

        builder.Property(e => e.Color)
            .HasMaxLength(50);

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(e => e.FamilyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure LunarDate as an owned entity
        builder.OwnsOne(e => e.LunarDate, lunarDateBuilder =>
        {
            lunarDateBuilder.Property(ld => ld.Day).HasColumnName("LunarDay");
            lunarDateBuilder.Property(ld => ld.Month).HasColumnName("LunarMonth");
            lunarDateBuilder.Property(ld => ld.IsLeapMonth).HasColumnName("LunarIsLeapMonth");
            // Explicitly map the foreign key shadow property to the same column as the owner's primary key
            // This should resolve the 'different columns' part of the error.
            lunarDateBuilder.WithOwner().HasForeignKey("Id"); // The FK shadow property is named 'Id' to match the owner's PK column
        });

        // CalendarType and RepeatRule are enums, mapped by default, but explicitly mark as required
        builder.Property(e => e.CalendarType)
            .IsRequired();

        builder.Property(e => e.RepeatRule)
            .IsRequired();
    }
}
