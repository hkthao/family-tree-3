using backend.Application.Common.Models;

namespace backend.Application.AI.Queries.GetLastUserPrompt;

/// <summary>
/// Query to retrieve the last user prompt for a specific member.
/// </summary>
public record GetLastUserPromptQuery : IRequest<Result<string?>>
{
    public Guid MemberId { get; init; }
}
