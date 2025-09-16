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
            Status = request.Status,
            Phone = request.Phone,
            Email = request.Email,
            Generation = request.Generation,
            DisplayOrder = request.DisplayOrder,
            FamilyId = ObjectId.Parse(request.FamilyId!),
            Description = request.Description
        };

        await _context.Members.InsertOneAsync(entity, cancellationToken: cancellationToken);

        return entity.Id.ToString();
    }
}
