using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Entity Framework Core cho thực thể VoiceGeneration.
/// </summary>
public class VoiceGenerationConfiguration : IEntityTypeConfiguration<VoiceGeneration>
{
    public void Configure(EntityTypeBuilder<VoiceGeneration> builder)
    {
        // Primary Key
        builder.HasKey(vg => vg.Id);

        // Properties
        builder.Property(vg => vg.VoiceProfileId)
            .IsRequired();

        builder.Property(vg => vg.Text)
            .HasMaxLength(4000) // Max length for text, can be text type in MySQL
            .IsRequired();

        builder.Property(vg => vg.AudioUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(vg => vg.Duration)
            .IsRequired();

        // Relationships
        builder.HasOne(vg => vg.VoiceProfile)
            .WithMany(vp => vp.VoiceGenerations)
            .HasForeignKey(vg => vg.VoiceProfileId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete VoiceGenerations when a VoiceProfile is deleted
    }
}
