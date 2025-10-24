using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.NotificationTemplates.Commands.UpdateNotificationTemplate;

/// <summary>
/// Lệnh để cập nhật một mẫu thông báo hiện có.
/// </summary>
public record UpdateNotificationTemplateCommand : IRequest
{
    /// <summary>
    /// ID của mẫu thông báo cần cập nhật.
    /// </summary>
    public Guid Id { get; init; }

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
    public bool IsActive { get; init; }
}

/// <summary>
/// Xử lý lệnh cập nhật mẫu thông báo.
/// </summary>
public class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateNotificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.NotificationTemplate), request.Id);
        }

        entity.EventType = request.EventType;
        entity.Channel = request.Channel;
        entity.Subject = request.Subject;
        entity.Body = request.Body;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
