using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Commands;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Member), request.Id);
        }

        // Comment: Apply Specification for validation before updating.
        // For example, you could have a specification to check if the new name is unique within the family.
        // var uniquenessSpec = new MemberUniqueNameInFamilySpecification(request.FirstName, request.LastName, request.FamilyId);
        // if (await SpecificationEvaluator<Member>.GetQuery(_context.Members, uniquenessSpec).AnyAsync(cancellationToken))
        // {
        //     throw new ValidationException("Member with this name already exists in the family.");
        // }

        entity.LastName = request.LastName;
        entity.FirstName = request.FirstName;
        entity.Nickname = request.Nickname;
        entity.DateOfBirth = request.DateOfBirth;
        entity.DateOfDeath = request.DateOfDeath;
        entity.PlaceOfBirth = request.PlaceOfBirth;
        entity.PlaceOfDeath = request.PlaceOfDeath;
        entity.Gender = request.Gender;
        entity.AvatarUrl = request.AvatarUrl;
        entity.Occupation = request.Occupation;
        entity.Biography = request.Biography;
        entity.FamilyId = request.FamilyId;
        entity.FatherId = request.FatherId;
        entity.MotherId = request.MotherId;
        entity.SpouseId = request.SpouseId;

        // Comment: Write-side invariant: Member is updated in the database context.
        await _context.SaveChangesAsync(cancellationToken);
    }
}