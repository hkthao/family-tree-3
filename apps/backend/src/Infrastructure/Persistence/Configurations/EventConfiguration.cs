using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        // Location property removed from Event entity
        // builder.Property(e => e.Location)
        //     .HasMaxLength(500);

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
        });

        // CalendarType and RepeatRule are enums, mapped by default, but explicitly mark as required
        builder.Property(e => e.CalendarType)
            .IsRequired();

        builder.Property(e => e.RepeatRule)
            .IsRequired();
    }
}
