using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Events.Commands.GenerateAndNotifyEvents;

public record GenerateAndNotifyEventsCommand : IRequest<Result<string>>
{
    public Guid? FamilyId { get; init; }
    public int? Year { get; init; }
}
