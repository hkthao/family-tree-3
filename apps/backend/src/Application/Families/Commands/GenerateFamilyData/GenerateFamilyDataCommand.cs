using backend.Application.Families.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Lệnh để tạo dữ liệu gia đình có cấu trúc từ văn bản người dùng bằng AI.
/// </summary>
public record GenerateFamilyDataCommand : IRequest<Result<AnalyzedResultDto>>
{
    /// <summary>
    /// Nội dung văn bản ngôn ngữ tự nhiên do người dùng cung cấp (ví dụ: mô tả thành viên gia đình, sự kiện).
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// ID phiên làm việc từ client.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// ID của gia đình liên quan đến yêu cầu tạo dữ liệu.
    /// </summary>
    public Guid FamilyId { get; init; }
}
