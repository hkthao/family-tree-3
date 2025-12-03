using backend.Application.Common.Models; // Added

namespace backend.Application.FamilyDicts.Commands.DeleteFamilyDict;

public record DeleteFamilyDictCommand(Guid Id) : IRequest<Result>;
