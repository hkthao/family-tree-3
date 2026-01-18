namespace backend.Application.Common.Models.LLMGateway;

public class LLMMessage
{
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
}
