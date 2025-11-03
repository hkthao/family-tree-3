using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Entity Framework Core cho thực thể User.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {

        // Cấu hình thuộc tính AuthProviderId
        builder.Property(u => u.AuthProviderId)
            .HasMaxLength(256) // Chiều dài hợp lý cho Auth0 'sub' claim
            .IsRequired();

        // Đảm bảo AuthProviderId là duy nhất
        builder.HasIndex(u => u.AuthProviderId)
            .IsUnique();

        // Cấu hình thuộc tính Email
        builder.Property(u => u.Email)
            .HasMaxLength(256) // Chiều dài hợp lý cho email
            .IsRequired();

        // Cấu hình mối quan hệ 1-1 với UserProfile
        builder.HasOne(u => u.Profile)
            .WithOne(up => up.User)
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Khi User bị xóa, UserProfile liên quan cũng bị xóa

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(36); // GUID string length

        builder.Property(u => u.LastModified)
            .IsRequired(false);

        builder.Property(u => u.LastModifiedBy)
            .HasMaxLength(36); // GUID string length
    }
}
