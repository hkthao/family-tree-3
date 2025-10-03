using MediatR;
using backend.Application.Families.Commands.Inputs;

namespace backend.Application.Families.Commands.CreateFamily;

public record CreateFamilyCommand : FamilyInput, IRequest<Guid>
{
}