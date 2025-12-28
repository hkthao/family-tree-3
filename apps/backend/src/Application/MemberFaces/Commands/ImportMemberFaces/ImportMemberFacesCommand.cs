using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using MediatR;
using System.Collections.Generic;

namespace backend.Application.MemberFaces.Commands.ImportMemberFaces;

public record ImportMemberFacesCommand(Guid FamilyId, List<ImportMemberFaceItemDto> Faces) : IRequest<Result<List<MemberFaceDto>>>;