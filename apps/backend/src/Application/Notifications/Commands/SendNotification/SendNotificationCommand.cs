using backend.Application.Common.Models;

namespace backend.Application.Notifications.Commands.SendNotification;

public record SendNotificationCommand : IRequest<Result>
{
    public string? WorkflowId { get; init; }
    public string? UserId { get; init; }
    public object? Payload { get; init; }
}
