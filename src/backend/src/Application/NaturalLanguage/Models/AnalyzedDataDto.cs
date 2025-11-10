using System.Collections.Generic;

namespace backend.Application.NaturalLanguage.Models;

/// <summary>
/// Đại diện cho dữ liệu đã được AI phân tích từ văn bản ngôn ngữ tự nhiên.
/// </summary>
public class AnalyzedDataDto
{
    /// <summary>
    /// Danh sách các thành viên được AI xác định từ văn bản.
    /// </summary>
    public List<MemberDataDto> Members { get; set; } = new List<MemberDataDto>();

    /// <summary>
    /// Danh sách các sự kiện được AI xác định từ văn bản.
    /// </summary>
    public List<EventDataDto> Events { get; set; } = new List<EventDataDto>();

    /// <summary>
    /// Thông tin phản hồi từ AI nếu có dữ liệu bị thiếu hoặc cần làm rõ.
    /// </summary>
    public string? Feedback { get; set; }
}

/// <summary>
/// Đại diện cho dữ liệu của một thành viên được AI xác định.
/// </summary>
public class MemberDataDto
{
    /// <summary>
    /// ID của thành viên (nếu đã tồn tại và được xác định).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Tên đầy đủ của thành viên.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Ngày sinh của thành viên (có thể là chuỗi để AI dễ dàng trả về).
    /// </summary>
    public string? DateOfBirth { get; set; }

    /// <summary>
    /// Ngày mất của thành viên (có thể là chuỗi để AI dễ dàng trả về).
    /// </summary>
    public string? DateOfDeath { get; set; }

    /// <summary>
    /// Giới tính của thành viên.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Danh sách các mối quan hệ được đề xuất cho thành viên này.
    /// </summary>
    public List<RelationshipDataDto> Relationships { get; set; } = new List<RelationshipDataDto>();
}

/// <summary>
/// Đại diện cho một mối quan hệ được AI đề xuất.
/// </summary>
public class RelationshipDataDto
{
    /// <summary>
    /// Loại mối quan hệ (ví dụ: "Father", "Mother", "Husband", "Wife", "Child").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Thông tin về thành viên liên quan đến mối quan hệ này.
    /// </summary>
    public MemberDataDto RelatedMember { get; set; } = null!;
}

/// <summary>
/// Đại diện cho dữ liệu của một sự kiện được AI xác định.
/// </summary>
public class EventDataDto
{
    /// <summary>
    /// Loại sự kiện (ví dụ: "Sinh", "Mất", "Kết hôn", "Kỷ niệm").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về sự kiện.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ngày của sự kiện (có thể là chuỗi để AI dễ dàng trả về).
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// Địa điểm diễn ra sự kiện.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Danh sách các thành viên liên quan đến sự kiện này.
    /// </summary>
    public List<string> RelatedMemberIds { get; set; } = new List<string>();
}
