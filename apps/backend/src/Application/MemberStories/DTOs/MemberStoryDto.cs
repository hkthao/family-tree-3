namespace backend.Application.MemberStories.DTOs; // Updated

public class MemberStoryDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    // Thêm thông tin thành viên
    public string MemberFullName { get; set; } = string.Empty;
    public string? MemberAvatarUrl { get; set; }
    public string? MemberGender { get; set; } // Giới tính (ví dụ: "Male", "Female", "Other", "Unknown")
}
