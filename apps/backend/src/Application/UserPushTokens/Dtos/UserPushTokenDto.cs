using backend.Application.Common.Dtos;

namespace backend.Application.UserPushTokens.DTOs;

public class UserPushTokenDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ExpoPushToken { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public bool IsActive { get; set; }
}
