using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class FamilyUserConfiguration : IEntityTypeConfiguration<FamilyUser>
{
    public void Configure(EntityTypeBuilder<FamilyUser> builder)
    {
        builder.HasKey(fu => new { fu.FamilyId, fu.UserId });

        builder.Property(fu => fu.Role)
            .IsRequired();

        builder.HasOne(fu => fu.Family)
            .WithMany(f => f.FamilyUsers) // Referencing the public property
            .HasForeignKey(fu => fu.FamilyId);

        builder.HasOne(fu => fu.User)
            .WithMany() // User no longer has a public FamilyUsers collection
            .HasForeignKey(fu => fu.UserId);
    }
}
