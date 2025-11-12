using backend.Domain.Enums;

namespace backend.Domain.ValueObjects;

public record FamilyUserUpdateInfo(Guid UserId, FamilyRole Role);
