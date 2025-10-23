using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.DeleteFamily;

public record DeleteFamilyCommand(Guid Id) : IRequest<Result>;
