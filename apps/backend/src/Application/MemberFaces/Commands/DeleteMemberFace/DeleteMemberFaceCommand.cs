using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.MemberFaces.Commands.DeleteMemberFace;

public record DeleteMemberFaceCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
}
