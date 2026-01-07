using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence;

public class FamilyFollowConfiguration : IEntityTypeConfiguration<FamilyFollow>
{
    public void Configure(EntityTypeBuilder<FamilyFollow> builder)
    {
        // Primary Key
        builder.HasKey(ff => ff.Id);

        // Properties
        builder.Property(ff => ff.UserId)
            .IsRequired();

        builder.Property(ff => ff.FamilyId)
            .IsRequired();

        builder.Property(ff => ff.IsFollowing)
            .IsRequired();

        // Relationships
        builder.HasOne(ff => ff.User)
            .WithMany() // Assuming User does not have a navigation property back to FamilyFollows
            .HasForeignKey(ff => ff.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ff => ff.Family)
            .WithMany() // Assuming Family does not have a navigation property back to FamilyFollows
            .HasForeignKey(ff => ff.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
