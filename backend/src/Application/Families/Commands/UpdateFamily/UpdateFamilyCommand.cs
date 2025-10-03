using MediatR;
using backend.Application.Families.Commands.Inputs;

namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : FamilyInput, IRequest
{
    public Guid Id { get; init; }
}