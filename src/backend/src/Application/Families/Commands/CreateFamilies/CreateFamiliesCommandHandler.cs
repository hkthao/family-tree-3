using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Families.Commands.CreateFamilies;

public class CreateFamiliesCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<CreateFamiliesCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;

    public async Task<Result<List<Guid>>> Handle(CreateFamiliesCommand request, CancellationToken cancellationToken)
    {
        var createdFamilyIds = new List<Guid>();

        var userProfile = await _context.UserProfiles.WithSpecification(new UserProfileByIdSpecification(_user.Id!.Value)).FirstOrDefaultAsync(cancellationToken);
        if (userProfile == null)
        {
            return Result<List<Guid>>.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        foreach (var familyDto in request.Families)
        {
            var familyId = Guid.NewGuid();
            var entity = new Family
            {
                Id = familyId,
                Name = familyDto.Name,
                Description = familyDto.Description,
                Address = familyDto.Address,
                AvatarUrl = familyDto.AvatarUrl,
                Visibility = familyDto.Visibility ?? "Public",
                TotalMembers = familyDto.TotalMembers,
                TotalGenerations = familyDto.TotalGenerations ?? 0,
                FamilyUsers = [new FamilyUser()
                {
                    FamilyId = familyId,
                    UserProfileId = userProfile.Id,
                    Role = FamilyRole.Manager
                }]
            };

            _context.Families.Add(entity);
            createdFamilyIds.Add(entity.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<Guid>>.Success(createdFamilyIds);
    }
}
