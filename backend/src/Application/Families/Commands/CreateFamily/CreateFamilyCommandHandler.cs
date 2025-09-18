using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = new Family
        {
            Name = request.Name!,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl
        };

        _context.Families.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}