using backend.Domain.Entities;
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
    }
}
