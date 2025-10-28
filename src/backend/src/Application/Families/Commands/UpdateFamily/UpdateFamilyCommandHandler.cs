using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_authorizationService.IsAdmin())
            {
                if (!_authorizationService.CanManageFamily(request.Id))
                {
                    return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
                }
            }

            var entity = await _context.Families.WithSpecification(new FamilyByIdSpecification(request.Id)).FirstOrDefaultAsync(cancellationToken);
            if (entity == null)
                return Result.Failure(string.Format(ErrorMessages.FamilyNotFound, request.Id), ErrorSources.NotFound);

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Address = request.Address;
            entity.AvatarUrl = request.AvatarUrl;
            entity.Visibility = request.Visibility;

            entity.AddDomainEvent(new FamilyUpdatedEvent(entity));

            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

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
