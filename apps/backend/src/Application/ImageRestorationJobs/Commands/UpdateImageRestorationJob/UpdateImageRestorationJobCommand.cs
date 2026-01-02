using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.ImageRestorationJobs.Commands.UpdateImageRestorationJob;

public record UpdateImageRestorationJobCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public RestorationStatus? Status { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RestoredImageUrl { get; init; }
}
