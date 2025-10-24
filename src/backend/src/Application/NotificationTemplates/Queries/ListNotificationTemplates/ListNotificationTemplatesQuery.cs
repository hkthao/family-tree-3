using backend.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.NotificationTemplates.Queries.ListNotificationTemplates;

/// <summary>
/// Truy vấn để lấy danh sách tất cả các mẫu thông báo.
/// </summary>
public record ListNotificationTemplatesQuery : IRequest<List<NotificationTemplateDto>>
{
}

/// <summary>
/// Xử lý truy vấn để lấy danh sách tất cả các mẫu thông báo.
/// </summary>
public class ListNotificationTemplatesQueryHandler : IRequestHandler<ListNotificationTemplatesQuery, List<NotificationTemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public ListNotificationTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationTemplateDto>> Handle(ListNotificationTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _context.NotificationTemplates
            .AsNoTracking()
            .Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                EventType = t.EventType,
                Channel = t.Channel,
                Subject = t.Subject,
                Body = t.Body,
                IsActive = t.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
