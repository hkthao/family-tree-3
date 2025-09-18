using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
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
            Metadata = request.Metadata
        };

        if (request.FullName != null)
        {
            entity.FullName = request.FullName;
        }

        _context.Members.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id.ToString();
    }
}