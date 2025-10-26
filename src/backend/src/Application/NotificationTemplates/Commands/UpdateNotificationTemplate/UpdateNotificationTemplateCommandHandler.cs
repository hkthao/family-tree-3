using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.NotificationTemplates.Commands.UpdateNotificationTemplate;

public class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateNotificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .FirstOrDefaultAsync(nt => nt.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure(string.Format(ErrorMessages.NotFound, nameof(NotificationTemplate)), ErrorSources.NotFound);
        }

        entity.EventType = request.EventType;
        entity.Channel = request.Channel;
        entity.Subject = request.Subject;
        entity.Body = request.Body;
        entity.Format = request.Format;
        entity.LanguageCode = request.LanguageCode;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
