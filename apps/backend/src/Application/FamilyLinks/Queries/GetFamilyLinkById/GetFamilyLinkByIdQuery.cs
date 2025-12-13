using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinkById;

public record GetFamilyLinkByIdQuery(Guid FamilyLinkId) : IRequest<Result<FamilyLinkDto>>;
