namespace backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;

/// <summary>
/// Đại diện cho dữ liệu của một mention đã được trích xuất.
/// </summary>
/// <param name="Id">ID duy nhất của thành viên được đề cập.</param>
/// <param name="DisplayName">Tên hiển thị của thành viên được đề cập.</param>
public record MentionData(string Id, string DisplayName);
