using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

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
            FullName = request.FullName,
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

        await _context.Members.InsertOneAsync(entity, cancellationToken: cancellationToken);

        return entity.Id.ToString();
    }
}
