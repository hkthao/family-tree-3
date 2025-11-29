using backend.Application.Common.Models;

namespace backend.Application.Faces.Commands.DeleteMemberFace;

public record DeleteMemberFaceCommand(Guid MemberFaceId) : IRequest<Result<Unit>>;
