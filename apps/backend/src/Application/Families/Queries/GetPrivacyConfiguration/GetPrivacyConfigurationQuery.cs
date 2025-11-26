using backend.Application.Common.Models;

namespace backend.Application.PrivacyConfigurations.Queries;

public record GetPrivacyConfigurationQuery(Guid FamilyId) : IRequest<Result<PrivacyConfigurationDto>>;
