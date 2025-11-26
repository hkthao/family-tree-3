using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamilyDetails;

public class GetFamilyDetailsQueryHandler(
    IApplicationDbContext context,
    IAuthorizationService authorizationService,
    IMapper mapper) // Inject IMapper
    : IRequestHandler<GetFamilyDetailsQuery, Result<FamilyDetailsDto>> // Changed return type
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMapper _mapper = mapper; // Store IMapper

    public async Task<Result<FamilyDetailsDto>> Handle(GetFamilyDetailsQuery request, CancellationToken cancellationToken) // Changed return type
    {
        // 1. Authorization
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<FamilyDetailsDto>.Failure($"Family with ID {request.FamilyId} not found.", ErrorSources.NotFound);
        }

        var succeeded = _authorizationService.CanAccessFamily(request.FamilyId);
        if (!succeeded)
        {
            return Result<FamilyDetailsDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 2. Retrieve Family Data using the specification
        var spec = new FamilyByIdWithAllDetailsSpecification(request.FamilyId);
        family = await _context.Families
            .WithSpecification(spec)
            .AsNoTracking() // Read-only query
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            // This case should ideally not happen if the first check passed, but for safety
            return Result<FamilyDetailsDto>.Failure($"Family with ID {request.FamilyId} not found after detailed fetch.", ErrorSources.NotFound);
        }

        // Map Family entity to FamilyDetailsDto
        var familyDetailsDto = _mapper.Map<FamilyDetailsDto>(family);

        return Result<FamilyDetailsDto>.Success(familyDetailsDto);
    }
}
