using backend.Application.Prompts.DTOs;
using MediatR;
using backend.Application.Common.Models;

namespace backend.Application.Prompts.Queries.GetPromptById;

public record GetPromptByIdQuery : IRequest<Result<PromptDto>>
{
    public Guid? Id { get; init; }
    public string? Code { get; init; }
}
