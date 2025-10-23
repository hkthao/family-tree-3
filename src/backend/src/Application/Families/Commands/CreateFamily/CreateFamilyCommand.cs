using backend.Application.Common.Models;
using backend.Application.Families.Commands.Inputs;

namespace backend.Application.Families.Commands.CreateFamily;

public record CreateFamilyCommand : FamilyInput, IRequest<Result<Guid>>
{
}
