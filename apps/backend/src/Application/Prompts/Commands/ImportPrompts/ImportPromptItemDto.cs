namespace backend.Application.Prompts.Commands.ImportPrompts;

/// <summary>
/// Đại diện cho đối tượng truyền dữ liệu (DTO) của một lời nhắc cần nhập.
/// </summary>
public record ImportPromptItemDto
{
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Content { get; init; } = null!;
    public string? Description { get; init; }
}
