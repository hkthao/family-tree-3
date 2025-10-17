using backend.Application.Common.Models; // Added for Result

namespace backend.Application.Families.Commands.DeleteFamily;

public record DeleteFamilyCommand(Guid Id) : IRequest<Result>;
