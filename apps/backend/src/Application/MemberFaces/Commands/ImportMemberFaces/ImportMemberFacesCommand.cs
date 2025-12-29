using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.Commands.ImportMemberFaces;

public record ImportMemberFacesCommand(Guid FamilyId, List<ImportMemberFaceItemDto> Faces) : IRequest<Result<List<MemberFaceDto>>>;
