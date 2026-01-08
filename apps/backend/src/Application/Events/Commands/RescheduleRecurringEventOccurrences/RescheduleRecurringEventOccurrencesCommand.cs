using backend.Application.Common.Models; // For Result

namespace backend.Application.Events.Commands.RescheduleRecurringEventOccurrences;

public record RescheduleRecurringEventOccurrencesCommand : IRequest<Result<string>>;
