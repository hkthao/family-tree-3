using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinks;

public record GetFamilyLinksQuery(Guid FamilyId) : IRequest<Result<List<FamilyLinkDto>>>;