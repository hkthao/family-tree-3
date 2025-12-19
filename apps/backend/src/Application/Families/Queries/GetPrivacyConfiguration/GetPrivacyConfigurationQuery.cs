using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetPrivacyConfiguration;

public record GetPrivacyConfigurationQuery(Guid FamilyId) : IRequest<Result<PrivacyConfigurationDto>>;
