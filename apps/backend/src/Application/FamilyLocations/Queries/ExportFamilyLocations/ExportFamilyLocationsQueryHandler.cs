using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using AutoMapper.QueryableExtensions;

namespace backend.Application.FamilyLocations.Queries.ExportFamilyLocations;

public class ExportFamilyLocationsQueryHandler : IRequestHandler<ExportFamilyLocationsQuery, Result<List<FamilyLocationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ExportFamilyLocationsQueryHandler> _logger;
    private readonly IMapper _mapper;

    public ExportFamilyLocationsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ExportFamilyLocationsQueryHandler> logger, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper;
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

        return Result<List<FamilyLocationDto>>.Success(familyLocations);
    }
}
