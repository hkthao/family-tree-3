using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.Queries.ExportMemberFaces;

public record ExportMemberFacesQuery(Guid FamilyId) : IRequest<Result<List<MemberFaceDto>>>;
