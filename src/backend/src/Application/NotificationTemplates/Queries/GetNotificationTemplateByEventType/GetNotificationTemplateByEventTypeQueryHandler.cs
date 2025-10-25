using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;

/// <summary>
/// Xử lý truy vấn để lấy mẫu thông báo dựa trên loại sự kiện và kênh.
/// </summary>
public class GetNotificationTemplateByEventTypeQueryHandler : IRequestHandler<GetNotificationTemplateByEventTypeQuery, Result<NotificationTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetNotificationTemplateByEventTypeQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<NotificationTemplateDto>> Handle(GetNotificationTemplateByEventTypeQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .AsNoTracking()
            .Where(t => t.EventType == request.EventType && t.Channel == request.Channel && t.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<NotificationTemplateDto>.Failure("Notification template not found.");
        }

        return Result<NotificationTemplateDto>.Success(_mapper.Map<NotificationTemplateDto>(entity));
    }
}
