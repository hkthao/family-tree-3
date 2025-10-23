using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.HasKey(up => up.UserProfileId);

        builder.HasOne(up => up.UserProfile)
            .WithOne()
            .HasForeignKey<UserPreference>(up => up.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
