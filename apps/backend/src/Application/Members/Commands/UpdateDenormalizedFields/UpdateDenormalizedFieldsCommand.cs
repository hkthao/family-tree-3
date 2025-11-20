using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Members.Commands.UpdateDenormalizedFields;

public record UpdateDenormalizedFieldsCommand(Guid FamilyId) : IRequest<Result>;
