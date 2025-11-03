using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {

        builder.Property(up => up.Theme)
            .IsRequired();

        builder.Property(up => up.Language)
            .IsRequired();

        builder.HasKey(up => up.UserId);
        
        builder.HasOne(up => up.User)
            .WithOne(u => u.Preference) // Specify the navigation property in User
            .HasForeignKey<UserPreference>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
