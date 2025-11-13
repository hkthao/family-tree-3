using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.ValueObjects;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
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

            entity.UpdateFamilyDetails(request.Name, request.Description, request.Address, request.AvatarUrl, request.Visibility, request.Code!);
            _context.FamilyUsers.RemoveRange(entity.FamilyUsers);
            var familyUserUpdateInfos = request.FamilyUsers.Select(fu => new FamilyUserUpdateInfo(fu.UserId, fu.Role));

            entity.UpdateFamilyUsers(familyUserUpdateInfos);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }
}
