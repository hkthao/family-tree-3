using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<DeleteEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Failure(string.Format(ErrorMessages.EventNotFound, request.Id), ErrorSources.NotFound);

        if (!_authorizationService.CanManageFamily(entity.FamilyId!.Value))
            return Result<bool>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        entity.AddDomainEvent(new Domain.Events.Events.EventDeletedEvent(entity));

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}
