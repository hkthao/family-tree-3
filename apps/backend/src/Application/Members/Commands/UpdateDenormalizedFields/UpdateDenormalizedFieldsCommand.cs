using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.UpdateDenormalizedFields;

public record UpdateDenormalizedFieldsCommand(Guid FamilyId) : IRequest<Result>;
