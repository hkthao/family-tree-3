using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.FamilyDicts.Commands.ImportFamilyDicts;

public class ImportFamilyDictsCommandHandler : IRequestHandler<ImportFamilyDictsCommand, IEnumerable<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ImportFamilyDictsCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Guid>> Handle(ImportFamilyDictsCommand request, CancellationToken cancellationToken)
    {
        var importedFamilyDictIds = new List<Guid>();

        foreach (var importDto in request.FamilyDicts)
        {
            var entity = new FamilyDict
            {
                // Id sẽ được tạo tự động bởi BaseEntity
                Name = importDto.Name,
                Type = importDto.Type,
                Description = importDto.Description,
                Lineage = importDto.Lineage,
                SpecialRelation = importDto.SpecialRelation,
                NamesByRegion = _mapper.Map<NamesByRegion>(importDto.NamesByRegion)
            };

            _context.FamilyDicts.Add(entity);
            importedFamilyDictIds.Add(entity.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return importedFamilyDictIds;
    }
}
