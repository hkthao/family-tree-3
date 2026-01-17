using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.UpdateMemberRelationships;

public record UpdateMemberRelationshipsCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; set; }
    public Guid FamilyId { get; set; } // Required for authorization and relationship service
    public Guid? FatherId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? HusbandId { get; set; }
    public Guid? WifeId { get; set; }
}
