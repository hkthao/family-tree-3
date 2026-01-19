using MediatR;
using backend.Application.Common.Models;

namespace backend.Application.MemberFaces.Commands.SyncMemberFacesToKnowledgeService;

public record SyncMemberFacesToKnowledgeServiceCommand : IRequest<Result<Unit>>
{
    public bool ForceResyncAll { get; init; } = false;
    public Guid? FamilyId { get; init; }
}
