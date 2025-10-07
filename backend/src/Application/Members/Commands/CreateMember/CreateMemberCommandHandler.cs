using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateMemberCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return Result<Guid>.Failure("User is not authenticated.");
        }

        var currentUserProfile = await _context.UserProfiles
            .WithSpecification(new UserProfileByAuth0IdSpec(_user.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUserProfile == null)
        {
            return Result<Guid>.Failure("User profile not found.");
        }

        // Check if the user has Manager role for the family
        var familyUser = currentUserProfile.FamilyUsers.FirstOrDefault(fu => fu.FamilyId == request.FamilyId);
        if (familyUser == null || familyUser.Role != FamilyRole.Manager)
        {
            return Result<Guid>.Failure("Access denied. Only family managers can create members.");
        }

        var entity = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Nickname = request.Nickname,
            DateOfBirth = request.DateOfBirth,
            DateOfDeath = request.DateOfDeath,
            PlaceOfBirth = request.PlaceOfBirth,
            PlaceOfDeath = request.PlaceOfDeath,
            Gender = request.Gender,
            AvatarUrl = request.AvatarUrl,
            Occupation = request.Occupation,
            Biography = request.Biography,
            FamilyId = request.FamilyId,
            IsRoot = request.IsRoot
        };

        if (request.IsRoot)
        {
            var currentRoot = await _context.Members
                .Where(m => m.FamilyId == request.FamilyId && m.IsRoot)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentRoot != null)
            {
                currentRoot.IsRoot = false;
            }
        }

        _context.Members.Add(entity);

        foreach (var relDto in request.Relationships)
        {
            entity.Relationships.Add(new Relationship
            {
                SourceMemberId = entity.Id,
                TargetMemberId = relDto.TargetMemberId,
                Type = relDto.Type,
                Order = relDto.Order
            });
        }

        // Comment: Write-side invariant: Member and Relationships are added to the database context.
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}