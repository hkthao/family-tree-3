using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using System;

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
        if (!request.FamilyId.HasValue)
        {
            throw new ArgumentNullException(nameof(request.FamilyId), "FamilyId cannot be null.");
        }

        var entity = new Member
        {
            DateOfBirth = request.DateOfBirth,
            DateOfDeath = request.DateOfDeath,
            Gender = request.Gender,
            AvatarUrl = request.AvatarUrl,
            PlaceOfBirth = request.PlaceOfBirth,
            Phone = request.Phone,
            Email = request.Email,
            Generation = request.Generation,
            Biography = request.Biography,
            Metadata = request.Metadata,
            FamilyId = request.FamilyId.Value
        };

        if (request.FullName != null)
        {
            entity.FullName = request.FullName;
        }

        _context.Members.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}