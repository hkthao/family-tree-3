using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = new Family
        {
            Name = request.Name!,
            Address = request.Address,
            LogoUrl = request.LogoUrl,
            Description = request.Description
        };

        await _context.Families.InsertOneAsync(entity, cancellationToken: cancellationToken);

        return entity.Id.ToString();
    }
}
