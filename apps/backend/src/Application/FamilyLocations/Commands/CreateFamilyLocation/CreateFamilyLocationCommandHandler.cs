using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities; // Needed for FamilyLocation entity
using backend.Domain.Events; // Needed for FamilyLocationCreatedEvent

namespace backend.Application.FamilyLocations.Commands.CreateFamilyLocation;

public class CreateFamilyLocationCommandHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<CreateFamilyLocationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<Guid>> Handle(CreateFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<FamilyLocation>(request);
        entity.AddDomainEvent(new FamilyLocationCreatedEvent(entity));

        _context.FamilyLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
