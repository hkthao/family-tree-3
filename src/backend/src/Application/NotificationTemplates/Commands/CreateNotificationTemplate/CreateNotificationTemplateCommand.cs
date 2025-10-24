using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using MediatR;

namespace backend.Application.NotificationTemplates.Commands.CreateNotificationTemplate;

/// <summary>
/// Lệnh để tạo một mẫu thông báo mới.
/// </summary>
public record CreateNotificationTemplateCommand : IRequest<Guid>
{
    /// <summary>
    /// Loại sự kiện mà mẫu thông báo này áp dụng (ví dụ: NewFamilyMember, RelationshipConfirmed).
    /// </summary>
    public NotificationType EventType { get; init; }

    /// <summary>
    /// Kênh thông báo mà mẫu này được sử dụng (ví dụ: InApp, Email, Firebase).
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Chủ đề của thông báo (đối với Email) hoặc tiêu đề (đối với InApp/Firebase).
    /// Có thể chứa các placeholder.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Nội dung chính của thông báo. Có thể chứa các placeholder.
    /// </summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// Cho biết mẫu thông báo này có đang hoạt động hay không.
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Xử lý lệnh tạo mẫu thông báo mới.
/// </summary>
public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.NotificationTemplate
        {
            EventType = request.EventType,
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body,
            IsActive = request.IsActive
        };

        _context.NotificationTemplates.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
