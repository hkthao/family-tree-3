using System;
using System.Collections.Generic;
using backend.Domain.Enums; // Add this using directive

namespace backend.Application.NaturalLanguage.Models;

/// <summary>
/// Đại diện cho dữ liệu đã được phân tích và chuyển đổi sang định dạng cuối cùng với Guid IDs.
/// </summary>
public class AnalyzedResultDto
{
    /// <summary>
    /// Danh sách các thành viên đã được chuyển đổi với Guid IDs.
    /// </summary>
    public List<MemberResultDto> Members { get; set; } = new List<MemberResultDto>();

    /// <summary>
    /// Danh sách các sự kiện đã được chuyển đổi với Guid IDs.
    /// </summary>
    public List<EventResultDto> Events { get; set; } = new List<EventResultDto>();

    /// <summary>
    /// Danh sách các mối quan hệ đã được chuyển đổi với Guid IDs.
    /// </summary>
    public List<RelationshipResultDto> Relationships { get; set; } = new List<RelationshipResultDto>();

    /// <summary>
    /// Thông tin phản hồi từ AI nếu có dữ liệu bị thiếu hoặc cần làm rõ.
    /// </summary>
    public string? Feedback { get; set; }
}

/// <summary>
/// Đại diện cho dữ liệu của một thành viên sau khi đã được xử lý và gán Guid ID.
/// </summary>
public class MemberResultDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string FullName => $"{LastName} {FirstName}".Trim();
    public string? DateOfBirth { get; set; }
    public string? DateOfDeath { get; set; }
    public string? Gender { get; set; }
    public int? Order { get; set; }
    public bool IsExisting { get; set; } = false;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Đại diện cho dữ liệu của một sự kiện sau khi đã được xử lý và gán Guid ID.
/// </summary>
public class EventResultDto
{
    public Guid Id { get; set; } // Event will also have a Guid ID
    public EventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Date { get; set; }
    public string? Location { get; set; }
    public List<Guid> RelatedMemberIds { get; set; } = new List<Guid>();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Đại diện cho một mối quan hệ giữa các thành viên.
/// </summary>
public class RelationshipResultDto
{
    public Guid SourceMemberId { get; set; }
    public Guid TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
    public string? ErrorMessage { get; set; }
}
