using backend.Application.Common.Models;

namespace backend.Application.Families.PrivacyConfigurations.Commands;

public record UpdatePrivacyConfigurationCommand(Guid FamilyId, List<string> PublicMemberProperties) : IRequest<Result<Unit>>;
