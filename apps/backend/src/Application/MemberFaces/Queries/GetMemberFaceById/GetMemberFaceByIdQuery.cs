using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto

namespace backend.Application.MemberFaces.Queries.GetMemberFaceById;

public record GetMemberFaceByIdQuery : IRequest<Result<MemberFaceDto>>
{
    public Guid Id { get; init; }
}
