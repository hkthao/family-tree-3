using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyUserConfiguration : IEntityTypeConfiguration<FamilyUser>
{
    public void Configure(EntityTypeBuilder<FamilyUser> builder)
    {
        builder.ToTable("family_users");

        builder.HasKey(fu => new { fu.FamilyId, fu.UserProfileId });

        builder.HasOne(fu => fu.Family)
            .WithMany(f => f.FamilyUsers)
            .HasForeignKey(fu => fu.FamilyId);

        builder.HasOne(fu => fu.UserProfile)
            .WithMany(up => up.FamilyUsers)
            .HasForeignKey(fu => fu.UserProfileId);
    }
}
