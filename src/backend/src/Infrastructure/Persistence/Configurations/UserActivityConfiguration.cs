using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        builder.ToTable("user_activity");

        builder.Property(t => t.UserProfileId)
            .HasColumnName("user_profile_id")
            .IsRequired();

        builder.Property(t => t.ActionType)
            .HasColumnName("action_type")
            .IsRequired();

        builder.Property(t => t.ActivitySummary)
            .HasColumnName("activity_summary")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.TargetId)
            .HasColumnName("target_id")
            .HasMaxLength(36);

        builder.Property(t => t.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(100);

        builder.Property(t => t.GroupId)
            .HasColumnName("group_id")
            .HasMaxLength(36);

        builder.Property(t => t.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(v, (System.Text.Json.JsonSerializerOptions?)null));
    }
}
