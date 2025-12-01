using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.ExportImport.Commands;
using backend.Application.Families.Specifications; // Assuming a specification for family with all details

namespace backend.Application.ExportImport.Queries;

public class GetFamilyExportQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService) : IRequestHandler<GetFamilyExportQuery, Result<FamilyExportDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<FamilyExportDto>> Handle(GetFamilyExportQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<FamilyExportDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Use a comprehensive specification to load family with all related entities
        var spec = new FamilyByIdWithAllDetailsSpecification(request.FamilyId);
        var family = await _context.Families
            .WithSpecification(spec)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<FamilyExportDto>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        var familyExportDto = _mapper.Map<FamilyExportDto>(family);

        return Result<FamilyExportDto>.Success(familyExportDto);
    }
}
