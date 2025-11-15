using backend.Application.Common.Models;
using backend.Application.NaturalLanguage.Models; // Add using directive

namespace backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;

/// <summary>
/// Lệnh để phân tích văn bản ngôn ngữ tự nhiên và tạo prompt cho AI Agent.
/// </summary>
public record AnalyzeNaturalLanguageCommand : IRequest<Result<AnalyzedResultDto>>
{
    /// <summary>
    /// Nội dung văn bản ngôn ngữ tự nhiên cần phân tích.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// ID phiên làm việc từ client.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
    public Guid FamilyId { get; init; }
}
