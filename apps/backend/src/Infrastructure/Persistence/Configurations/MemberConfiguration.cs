using backend.Domain.Entities;
using backend.Domain.ValueObjects; // Add this using statement
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {

        builder.Property(m => m.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Nickname)
            .HasMaxLength(100);

        builder.Property(m => m.Gender)
            .HasMaxLength(20);

        builder.Property(m => m.PlaceOfBirth)
            .HasMaxLength(200);

        builder.Property(m => m.PlaceOfDeath)
            .HasMaxLength(200);

        builder.Property(m => m.Phone)
            .HasMaxLength(20);

        builder.Property(m => m.Email)
            .HasMaxLength(100);

        builder.Property(m => m.Address)
            .HasMaxLength(500);

        builder.Property(m => m.Occupation)
            .HasMaxLength(200);

        builder.Property(m => m.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(m => m.Biography)
            .HasMaxLength(4000);

        builder.Property(m => m.FamilyId)
            .IsRequired();

        builder.HasIndex(m => m.Code)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        builder.HasOne(m => m.Family)
            .WithMany(f => f.Members) // Referencing the public property
            .HasForeignKey(m => m.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.Order)
            .IsRequired(false); // Make Order optional

        builder.OwnsOne(m => m.LunarDateOfDeath, lunarDate =>
        {
            lunarDate.Property(ld => ld.Day).HasColumnName("LunarDateOfDeath_Day");
            lunarDate.Property(ld => ld.Month).HasColumnName("LunarDateOfDeath_Month");
            lunarDate.Property(ld => ld.IsLeapMonth).HasColumnName("LunarDateOfDeath_IsLeapMonth");
            lunarDate.Property(ld => ld.IsEstimated).HasColumnName("LunarDateOfDeath_IsEstimated");
            lunarDate.WithOwner().HasForeignKey("Id"); // Explicitly use the 'Id' of the owning Member as the foreign key
        });
    }
}
