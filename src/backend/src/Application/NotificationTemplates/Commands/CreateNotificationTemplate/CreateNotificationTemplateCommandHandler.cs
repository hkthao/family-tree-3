using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.NotificationTemplates.Commands.CreateNotificationTemplate;

public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = new NotificationTemplate
        {
            EventType = request.EventType,
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body,
            IsActive = request.IsActive
        };

        _context.NotificationTemplates.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
