using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.FamilyDicts.Commands.ImportFamilyDicts;

public class ImportFamilyDictsCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService) : IRequestHandler<ImportFamilyDictsCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<List<Guid>>> Handle(ImportFamilyDictsCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            return Result<List<Guid>>.Forbidden("Chỉ quản trị viên mới được phép nhập FamilyDict.");
        }

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

        return Result<List<Guid>>.Success(importedFamilyDictIds);
    }
}
