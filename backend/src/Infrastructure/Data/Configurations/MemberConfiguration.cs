using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.Property(t => t.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Gender)
            .HasMaxLength(10);

        builder.Property(t => t.Phone)
            .HasMaxLength(20);

        builder.Property(t => t.Email)
            .HasMaxLength(100);

        builder.Property(t => t.PlaceOfBirth)
            .HasMaxLength(200);

        builder.Property(t => t.PlaceOfDeath)
            .HasMaxLength(200);

        // Assuming FamilyId is a string in the Domain entity, but Guid in DB
        // This might need adjustment if FamilyId in Member entity is still string
        // For now, assuming it's a string that stores a Guid.
        builder.Property(t => t.FamilyId)
            .IsRequired();
    }
}
