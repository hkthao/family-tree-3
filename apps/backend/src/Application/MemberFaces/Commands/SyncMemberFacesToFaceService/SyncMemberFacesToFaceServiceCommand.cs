using backend.Application.Common.Models;

namespace backend.Application.MemberFaces.Commands.SyncMemberFacesToFaceService;

public record SyncMemberFacesToFaceServiceCommand : IRequest<Result<Unit>>
{
    public Guid? FamilyId { get; init; }
}
