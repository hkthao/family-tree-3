namespace FamilyTree.Application.Families.Commands.CreateFamily;

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
            Name = request.Name,
            Description = request.Description,
            Address = request.Address
        };

        // TODO: Add domain event if needed
        // entity.AddDomainEvent(new FamilyCreatedEvent(entity));

        _context.Families.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}