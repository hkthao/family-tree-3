using backend.Application.Common.Models;

namespace backend.Application.MemberFaces.Commands.SyncMemberFacesToKnowledgeService;

public record SyncMemberFacesToKnowledgeServiceCommand : IRequest<Result<Unit>>
{
    public Guid? FamilyId { get; init; }
}
