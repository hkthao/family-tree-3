using backend.Application.Common.Models;
using backend.Application.MemberFaces.Queries.MemberFaces; // For MemberFaceDto

namespace backend.Application.MemberFaces.Queries.GetMemberFaceById;

public record GetMemberFaceByIdQuery : IRequest<Result<MemberFaceDto>>
{
    public Guid Id { get; init; }
}
