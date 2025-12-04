using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Prompts.Commands.UpdatePrompt;

public record UpdatePromptCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Content { get; init; } = null!;
    public string? Description { get; init; }
}
