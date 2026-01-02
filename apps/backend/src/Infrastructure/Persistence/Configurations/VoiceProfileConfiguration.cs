using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Entity Framework Core cho thực thể VoiceProfile.
/// </summary>
public class VoiceProfileConfiguration : IEntityTypeConfiguration<VoiceProfile>
{
    public void Configure(EntityTypeBuilder<VoiceProfile> builder)
    {
        // Primary Key
        builder.HasKey(vp => vp.Id);

        // Properties
        builder.Property(vp => vp.MemberId)
            .IsRequired();

        builder.Property(vp => vp.Label)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vp => vp.AudioUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(vp => vp.DurationSeconds)
            .IsRequired();

        builder.Property(vp => vp.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(vp => vp.Consent)
            .IsRequired();

        builder.Property(vp => vp.Status)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        // Relationships
        builder.HasOne(vp => vp.Member)
            .WithMany() // Assuming Member does not explicitly list VoiceProfiles collection
            .HasForeignKey(vp => vp.MemberId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete VoiceProfiles when a Member is deleted

        // One-to-many relationship with VoiceGeneration
        builder.HasMany(vp => vp.VoiceGenerations)
            .WithOne(vg => vg.VoiceProfile)
            .HasForeignKey(vg => vg.VoiceProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
