using backend.Application.Common.Models;  for Result

namespace backend.Application.Families.Commands.DeleteFamily;

public record DeleteFamilyCommand(Guid Id) : IRequest<Result>;
