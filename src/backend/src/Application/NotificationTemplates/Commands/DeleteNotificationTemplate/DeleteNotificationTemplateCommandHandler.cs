using backend.Application.Common.Models;
using MediatR;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Exceptions;
using backend.Domain.Entities;

namespace backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;

public class DeleteNotificationTemplateCommandHandler : IRequestHandler<DeleteNotificationTemplateCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteNotificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .FirstOrDefaultAsync(nt => nt.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure(new NotFoundException(nameof(NotificationTemplate), request.Id).Message);
        }

        _context.NotificationTemplates.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
