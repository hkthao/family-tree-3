using backend.Application.Common.Models;
using backend.Application.Members.DTOs; // Changed to DTOs for MemberImportDto
using MediatR;

namespace backend.Application.Members.Commands.ImportMembers;

public record ImportMembersCommand(Guid FamilyId, List<MemberImportDto> Members) : IRequest<Result<Unit>>; 
