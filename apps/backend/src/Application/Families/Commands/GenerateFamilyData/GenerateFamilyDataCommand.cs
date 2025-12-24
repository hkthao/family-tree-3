using backend.Application.Common.Models;
using backend.Application.AI.DTOs; // Add this using directive

namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Lệnh để tạo nội dung bằng AI dựa trên đầu vào của người dùng và loại yêu cầu.
/// </summary>
public record GenerateFamilyDataCommand : IRequest<Result<CombinedAiContentDto>>
{
    /// <summary>
    /// ID của gia đình liên quan đến yêu cầu.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// Đầu vào văn bản từ người dùng cho AI.
    /// </summary>
    public string ChatInput { get; init; } = null!;

}
