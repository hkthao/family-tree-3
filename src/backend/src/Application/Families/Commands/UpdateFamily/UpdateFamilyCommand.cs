using backend.Application.Common.Models;
using backend.Application.Families.Commands.Inputs;

namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : FamilyInput, IRequest<Result>
{
    public Guid Id { get; init; }
    public IList<FamilyUserDto> FamilyUsers { get; init; } = [];
}
