namespace backend.Application.Prompts.DTOs;

public class PromptDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; } = false; // Flag to indicate if some properties were hidden due to privacy
}
