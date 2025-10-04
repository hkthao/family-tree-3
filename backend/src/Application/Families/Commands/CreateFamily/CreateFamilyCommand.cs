using backend.Application.Families.Commands.Inputs;
using backend.Application.Common.Models; // Added for Result<T>

namespace backend.Application.Families.Commands.CreateFamily;

public record CreateFamilyCommand : FamilyInput, IRequest<Result<Guid>>
{
}