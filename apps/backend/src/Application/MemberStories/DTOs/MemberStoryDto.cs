namespace backend.Application.MemberStories.DTOs;

using System;
using System.Collections.Generic;
using backend.Domain.Enums;

public class MemberStoryImageDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public string? ResizedImageUrl { get; set; }
    public string? Caption { get; set; }
}

public class MemberStoryDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    // Thêm thông tin thành viên
    public string MemberFullName { get; set; } = string.Empty;
    public string? MemberAvatarUrl { get; set; }
    public string? MemberGender { get; set; } // Giới tính (ví dụ: "Male", "Female", "Other", "Unknown")
    public DateTime Created { get; set; }
    public Guid? FamilyId { get; set; }
    public int? Year { get; set; }
    public string? TimeRangeDescription { get; set; }
    public bool IsYearEstimated { get; set; }
    public LifeStage LifeStage { get; set; }
    public string? Location { get; set; }
    public ICollection<MemberStoryImageDto> MemberStoryImages { get; set; } = new List<MemberStoryImageDto>();
}
