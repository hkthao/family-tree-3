using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyUserConfiguration : IEntityTypeConfiguration<FamilyUser>
{
    public void Configure(EntityTypeBuilder<FamilyUser> builder)
    {
        builder.HasKey(fu => new { fu.FamilyId, fu.UserProfileId });

        builder.HasOne(fu => fu.Family)
            .WithMany(f => f.FamilyUsers)
            .HasForeignKey(fu => fu.FamilyId)
            .OnDelete(DeleteBehavior.Cascade); // If a family is deleted, its user associations are also deleted

        builder.HasOne(fu => fu.UserProfile)
            .WithMany(up => up.FamilyUsers)
            .HasForeignKey(fu => fu.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade); // If a user profile is deleted, their family associations are also deleted

        builder.Property(fu => fu.Role)
            .IsRequired();
    }
}