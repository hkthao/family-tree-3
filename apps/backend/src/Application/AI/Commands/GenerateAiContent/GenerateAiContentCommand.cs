using System.Dynamic;
using backend.Application.Common.Models;

namespace backend.Application.AI.Commands.GenerateAiContent;

/// <summary>
/// Lệnh để tạo nội dung bằng AI dựa trên đầu vào của người dùng và loại yêu cầu.
/// </summary>
public record GenerateAiContentCommand : IRequest<Result<ExpandoObject>>
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
