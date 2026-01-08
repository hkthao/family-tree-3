using backend.Application.Common.Models; // For Result

namespace backend.Application.Events.Commands.GenerateEventOccurrences;

public record GenerateEventOccurrencesCommand : IRequest<Result<string>>
{
    public int Year { get; init; }
    public Guid? FamilyId { get; init; }
}
