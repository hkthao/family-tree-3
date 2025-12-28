using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.FamilyLocations.Queries.ExportFamilyLocations;

public class ExportFamilyLocationsQueryHandler : IRequestHandler<ExportFamilyLocationsQuery, Result<List<FamilyLocationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ExportFamilyLocationsQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IPrivacyService _privacyService;

    public ExportFamilyLocationsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ExportFamilyLocationsQueryHandler> logger, IMapper mapper, IPrivacyService privacyService)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper;
        _privacyService = privacyService;
    }

    public async Task<Result<List<FamilyLocationDto>>> Handle(ExportFamilyLocationsQuery request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin hoặc quản lý gia đình mới có thể xuất vị trí gia đình
        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(request.FamilyId))
        {
            _logger.LogWarning("Người dùng không có quyền cố gắng xuất vị trí gia đình cho FamilyId {FamilyId}.", request.FamilyId);
            return Result<List<FamilyLocationDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var familyLocations = await _context.FamilyLocations
            .Where(fl => fl.FamilyId == request.FamilyId)
            .AsNoTracking()
            .ProjectTo<FamilyLocationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var filteredFamilyLocations = new List<FamilyLocationDto>();
        foreach (var familyLocationDto in familyLocations)
        {
            filteredFamilyLocations.Add(await _privacyService.ApplyPrivacyFilter(familyLocationDto, request.FamilyId, cancellationToken));
        }

        return Result<List<FamilyLocationDto>>.Success(filteredFamilyLocations);
    }
}
