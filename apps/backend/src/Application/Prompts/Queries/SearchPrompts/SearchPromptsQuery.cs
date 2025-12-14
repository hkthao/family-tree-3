using backend.Application.Common.Models;
using backend.Application.Prompts.DTOs; // Add this line

namespace backend.Application.Prompts.Queries.SearchPrompts;

public record SearchPromptsQuery : PaginatedQuery, IRequest<Result<PaginatedList<PromptDto>>>
{
    public string? SearchQuery { get; init; }
}
