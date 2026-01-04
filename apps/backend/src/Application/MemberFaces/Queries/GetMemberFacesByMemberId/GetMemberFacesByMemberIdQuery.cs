using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.Queries.GetMemberFacesByMemberId;

public record GetMemberFacesByMemberIdQuery : IRequest<Result<IEnumerable<MemberFaceDto>>>
{
    public Guid MemberId { get; init; }
}