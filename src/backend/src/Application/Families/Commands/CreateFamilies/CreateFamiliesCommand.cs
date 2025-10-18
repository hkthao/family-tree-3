using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.CreateFamilies;

public record CreateFamiliesCommand(List<FamilyDto> Families) : IRequest<Result<List<Guid>>>;
