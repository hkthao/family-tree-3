using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Data.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.Ignore(t => t.FullName);

            builder.Property(t => t.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(t => t.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(t => t.Nickname)
                .HasMaxLength(100);

            builder.Property(t => t.Gender)
                .HasMaxLength(10);

            builder.Property(t => t.PlaceOfBirth)
                .HasMaxLength(200);

            builder.Property(t => t.PlaceOfDeath)
                .HasMaxLength(200);

            builder.Property(t => t.Occupation)
                .HasMaxLength(100);

            builder.Property(t => t.FamilyId)
                .IsRequired();
        }
    }
}
