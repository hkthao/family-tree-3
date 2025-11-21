using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.FamilyDicts.Commands.UpdateFamilyDict;

public class UpdateFamilyDictCommandHandler : IRequestHandler<UpdateFamilyDictCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateFamilyDictCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateFamilyDictCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FamilyDicts
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(FamilyDict), request.Id.ToString());
        }

        entity.Name = request.Name;
        entity.Type = request.Type;
        entity.Description = request.Description;
        entity.Lineage = request.Lineage;
        entity.SpecialRelation = request.SpecialRelation;

        // Update NamesByRegion
        if (entity.NamesByRegion == null)
        {
            entity.NamesByRegion = _mapper.Map<NamesByRegion>(request.NamesByRegion);
        }
        else
        {
            _mapper.Map(request.NamesByRegion, entity.NamesByRegion);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
