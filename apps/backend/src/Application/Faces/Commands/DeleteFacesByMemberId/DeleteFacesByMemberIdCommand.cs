using backend.Application.Common.Models;

namespace backend.Application.Faces.Commands.DeleteFacesByMemberId;

public record DeleteFacesByMemberIdCommand(Guid MemberId) : IRequest<Result<Unit>>;
