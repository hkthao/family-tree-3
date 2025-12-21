using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Entity Framework Core cho thực thể FamilyLimitConfiguration.
/// </summary>
public class FamilyLimitConfigurationConfiguration : IEntityTypeConfiguration<FamilyLimitConfiguration>
{
    public void Configure(EntityTypeBuilder<FamilyLimitConfiguration> builder)
    {
        // Khóa chính
        builder.HasKey(fc => fc.Id);

        // Đánh dấu FamilyId là bắt buộc và là khóa ngoại đến Family
        builder.Property(fc => fc.FamilyId)
            .IsRequired();

        // Cấu hình mối quan hệ 1-1 giữa Family và FamilyLimitConfiguration
        // Một Family có một FamilyLimitConfiguration, và FamilyLimitConfiguration thuộc về một Family.
        // Cascade delete không được áp dụng vì FamilyLimitConfiguration là một phần của Family aggregate.
        builder.HasOne(fc => fc.Family)
            .WithOne(f => f.FamilyLimitConfiguration)
            .HasForeignKey<FamilyLimitConfiguration>(fc => fc.FamilyId)
            .OnDelete(DeleteBehavior.Cascade); // Khi Family bị xóa, FamilyLimitConfiguration cũng bị xóa

        // Cấu hình các thuộc tính
        builder.Property(fc => fc.MaxMembers)
            .IsRequired()
            .HasDefaultValue(50); // Giá trị mặc định

        builder.Property(fc => fc.MaxStorageMb)
            .IsRequired()
            .HasDefaultValue(1024); // Giá trị mặc định (1GB)

        // Đảm bảo FamilyId là duy nhất để mỗi Family chỉ có một FamilyLimitConfiguration
        builder.HasIndex(fc => fc.FamilyId)
            .IsUnique();
    }
}
