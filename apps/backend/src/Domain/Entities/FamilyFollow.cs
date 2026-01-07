using System;
using System.Collections.Generic;
using System.Linq;
using backend.Domain.Enums; // Assuming NotificationType is in Enums
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class FamilyFollow : BaseAuditableEntity
{
    public Guid UserId { get; set; } // Foreign key to User.Id (Guid)
    public Guid FamilyId { get; set; }

    public bool IsFollowing { get; set; } = true;

    // New notification preference properties
    public bool NotifyDeathAnniversary { get; set; } = false; // 🕯️ Ngày giỗ sắp tới
    public bool NotifyBirthday { get; set; } = false; // 🎂 Sinh nhật thành viên
    public bool NotifyEvent { get; set; } = false; // 📅 Sự kiện gia đình

    // Navigation properties
    public Family Family { get; set; } = null!;
    public User User { get; set; } = null!; // Foreign key to User

    private FamilyFollow() { } // Private constructor for EF Core and internal use

    public static FamilyFollow Create(Guid userId, Guid familyId)
    {
        return new FamilyFollow
        {
            UserId = userId,
            FamilyId = familyId,
            IsFollowing = true, // Mặc định là đang theo dõi khi tạo mới
            NotifyDeathAnniversary = true, // Mặc định bật thông báo Ngày giỗ
            NotifyBirthday = true, // Mặc định bật thông báo Sinh nhật
            NotifyEvent = true // Mặc định bật thông báo Sự kiện
        };
    }

    public void SetIsFollowing(bool isFollowing)
    {
        IsFollowing = isFollowing;
    }
}
