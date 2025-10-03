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
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            AvatarUrl = request.AvatarUrl,
            Visibility = request.Visibility
        };

        _context.Families.Add(entity);

        // Comment: Write-side invariant: Family is added to the database context.
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}