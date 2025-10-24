using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;

/// <summary>
/// Truy vấn để lấy mẫu thông báo dựa trên loại sự kiện và kênh.
/// </summary>
public record GetNotificationTemplateByEventTypeQuery : IRequest<NotificationTemplateDto?>
{
    /// <summary>
    /// Loại sự kiện của mẫu thông báo.
    /// </summary>
    public NotificationType EventType { get; init; }

    /// <summary>
    /// Kênh thông báo của mẫu thông báo.
    /// </summary>
    public NotificationChannel Channel { get; init; }
}

/// <summary>
/// Xử lý truy vấn để lấy mẫu thông báo dựa trên loại sự kiện và kênh.
/// </summary>
public class GetNotificationTemplateByEventTypeQueryHandler : IRequestHandler<GetNotificationTemplateByEventTypeQuery, NotificationTemplateDto?>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationTemplateByEventTypeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplateDto?> Handle(GetNotificationTemplateByEventTypeQuery request, CancellationToken cancellationToken)
    {
        return await _context.NotificationTemplates
            .AsNoTracking()
            .Where(t => t.EventType == request.EventType && t.Channel == request.Channel && t.IsActive)
            .Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                EventType = t.EventType,
                Channel = t.Channel,
                Subject = t.Subject,
                Body = t.Body,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
