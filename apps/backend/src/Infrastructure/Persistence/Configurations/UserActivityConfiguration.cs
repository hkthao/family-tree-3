using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserActivities) // Specify the navigation property in User
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade); // If a User is deleted, their activities are also deleted

        builder.Property(t => t.ActionType)
            .IsRequired();

        builder.Property(t => t.ActivitySummary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.TargetId)
            .HasMaxLength(36);

        builder.Property(t => t.TargetType)
            .HasMaxLength(100);

        builder.Property(t => t.GroupId)
            .HasMaxLength(36);

        builder.Property(t => t.Metadata)
            .HasColumnType("json")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(v, (System.Text.Json.JsonSerializerOptions?)null));
    }
}
