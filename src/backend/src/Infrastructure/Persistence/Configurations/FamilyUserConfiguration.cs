using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyUserConfiguration : IEntityTypeConfiguration<FamilyUser>
{
    public void Configure(EntityTypeBuilder<FamilyUser> builder)
    {
        builder.ToTable("family_user");

        builder.HasKey(fu => new { fu.FamilyId, fu.UserProfileId });

        builder.Property(fu => fu.FamilyId).HasColumnName("family_id");
        builder.Property(fu => fu.UserProfileId).HasColumnName("user_profile_id");

        builder.Property(fu => fu.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.HasOne(fu => fu.Family)
            .WithMany(f => f.FamilyUsers)
            .HasForeignKey(fu => fu.FamilyId);

        builder.HasOne(fu => fu.UserProfile)
            .WithMany(up => up.FamilyUsers)
            .HasForeignKey(fu => fu.UserProfileId);    }
}
