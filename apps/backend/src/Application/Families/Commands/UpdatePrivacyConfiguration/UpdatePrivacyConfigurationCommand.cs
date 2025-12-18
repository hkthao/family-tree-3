using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.UpdatePrivacyConfiguration;

public record UpdatePrivacyConfigurationCommand(Guid FamilyId, List<string> PublicMemberProperties) : IRequest<Result<Unit>>;
