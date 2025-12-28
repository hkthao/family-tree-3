using backend.Application.Common.Models;
using System.Collections.Generic; // Add this using directive for List

namespace backend.Application.Families.Commands.UpdatePrivacyConfiguration;

public record UpdatePrivacyConfigurationCommand(
    Guid FamilyId,
    List<string> PublicMemberProperties,
    List<string> PublicEventProperties,
    List<string> PublicFamilyProperties,
    List<string> PublicFamilyLocationProperties,
    List<string> PublicMemoryItemProperties,
    List<string> PublicMemberFaceProperties,
    List<string> PublicFoundFaceProperties
) : IRequest<Result<Unit>>;
