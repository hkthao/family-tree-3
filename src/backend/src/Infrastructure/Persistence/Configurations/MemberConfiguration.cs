using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Code)
            .IsUnique();

        builder.HasOne(m => m.Family)
            .WithMany()
            .HasForeignKey(m => m.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
