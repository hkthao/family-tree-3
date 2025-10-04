using backend.Application.Families.Commands.Inputs;
using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : FamilyInput, IRequest<Result>
{
    public Guid Id { get; init; }
}