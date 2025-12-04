using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Prompts.Commands.CreatePrompt;

public record CreatePromptCommand : IRequest<Result<Guid>>
{
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Content { get; init; } = null!;
    public string? Description { get; init; }
}
