using System.Collections.Generic;
using backend.Domain.Enums; // Add this using directive

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
    /// Danh sách các mối quan hệ được AI xác định từ văn bản.
    /// </summary>
    public List<RelationshipDataDto> Relationships { get; set; } = new List<RelationshipDataDto>();

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
    /// ID nội bộ của thành viên (nếu đã tồn tại trong hệ thống).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Mã (Code) của thành viên (nếu đã tồn tại và được đề cập).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Họ của thành viên.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Tên của thành viên.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Tên đầy đủ của thành viên (tính toán từ LastName và FirstName).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

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
    /// Thứ tự của thành viên trong số các anh chị em (nếu là con).
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Cờ đánh dấu thành viên đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    public bool IsExisting { get; set; } = false;

    /// <summary>
    /// Thông báo lỗi xác thực cụ thể cho thành viên này.
    /// </summary>
    public string? ErrorMessage { get; set; }
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

    /// <summary>
    /// Thông báo lỗi xác thực cụ thể cho sự kiện này.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Đại diện cho một mối quan hệ được AI xác định.
/// </summary>
public class RelationshipDataDto
{
    /// <summary>
    /// ID của thành viên nguồn (ví dụ: cha, mẹ, chồng, vợ).
    /// </summary>
    public string SourceMemberId { get; set; } = null!;

    /// <summary>
    /// ID của thành viên đích (ví dụ: con, vợ, chồng).
    /// </summary>
    public string TargetMemberId { get; set; } = null!;

    /// <summary>
    /// Loại mối quan hệ (ví dụ: Parent, Child, Spouse).
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Thứ tự của mối quan hệ (ví dụ: thứ tự con).
    /// </summary>
    public int? Order { get; set; }
}
