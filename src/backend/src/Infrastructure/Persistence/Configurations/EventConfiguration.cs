using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("event");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Created).HasColumnName("created");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.LastModified).HasColumnName("last_modified");
        builder.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(e => e.StartDate)
            .HasColumnName("start_date");

        builder.Property(e => e.EndDate)
            .HasColumnName("end_date");

        builder.Property(e => e.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(e => e.FamilyId)
            .HasColumnName("family_id");

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(e => e.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(e => e.FamilyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
