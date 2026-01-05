using backend.Application.Common.Models;
using backend.Application.Families.Commands.Inputs;

namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : FamilyInput, IRequest<Result<Guid>>
{
    public Guid Id { get; init; }
    public IList<Guid> ManagerIds { get; set; } = [];
    public IList<Guid> ViewerIds { get; set; } = [];
    public Guid? LocationId { get; set; }
}
