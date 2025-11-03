using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser, IDateTime dateTime) : IRequestHandler<DeleteFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_authorizationService.CanManageFamily(request.Id))
            {
                return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }

            var entity = await _context.Families.WithSpecification(new FamilyByIdSpecification(request.Id)).FirstOrDefaultAsync(cancellationToken);
            if (entity == null)
                return Result.Failure(string.Format(ErrorMessages.FamilyNotFound, request.Id), ErrorSources.NotFound);

            entity.IsDeleted = true;
            entity.DeletedBy = _currentUser.UserId.ToString();
            entity.DeletedDate = _dateTime.Now;

            entity.AddDomainEvent(new FamilyDeletedEvent(entity));

            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            entity.AddDomainEvent(new FamilyStatsUpdatedEvent(request.Id));

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Database);
        }
        catch (Exception ex)
        {
            return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }
}
