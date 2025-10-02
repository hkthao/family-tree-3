using System;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
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
            FatherId = request.FatherId,
            MotherId = request.MotherId,
            SpouseId = request.SpouseId
        };

        _context.Members.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
