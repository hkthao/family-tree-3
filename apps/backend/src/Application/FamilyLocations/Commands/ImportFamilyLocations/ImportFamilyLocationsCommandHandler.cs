using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.FamilyLocations.Commands.ImportFamilyLocations;

public class ImportFamilyLocationsCommandHandler : IRequestHandler<ImportFamilyLocationsCommand, Result<List<FamilyLocationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ImportFamilyLocationsCommandHandler> _logger;
    private readonly IMapper _mapper;

    public ImportFamilyLocationsCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ImportFamilyLocationsCommandHandler> logger, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<FamilyLocationDto>>> Handle(ImportFamilyLocationsCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin hoặc quản lý gia đình mới có thể nhập vị trí gia đình
        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(request.FamilyId))
        {
            _logger.LogWarning("Người dùng không có quyền cố gắng nhập vị trí gia đình cho FamilyId {FamilyId}.", request.FamilyId);
            return Result<List<FamilyLocationDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Xử lý danh sách nhập rỗng một cách rõ ràng
        if (request.Locations == null || !request.Locations.Any())
        {
            return Result<List<FamilyLocationDto>>.Success(new List<FamilyLocationDto>());
        }

        var importedFamilyLocations = new List<FamilyLocation>();

        foreach (var importLocationItemDto in request.Locations)
        {
            // Kiểm tra xem FamilyLocation đã tồn tại chưa (FamilyId và Location.Name)
            var existingFamilyLocation = await _context.FamilyLocations
                .Include(fl => fl.Location)
                .FirstOrDefaultAsync(fl => fl.FamilyId == request.FamilyId && fl.Location.Name == importLocationItemDto.LocationName, cancellationToken);

            if (existingFamilyLocation != null)
            {
                _logger.LogInformation("Vị trí gia đình với tên '{LocationName}' đã tồn tại cho FamilyId {FamilyId}. Bỏ qua nhập.", importLocationItemDto.LocationName, request.FamilyId);
                continue;
            }

            // Create Location entity
            var location = new Location(
                importLocationItemDto.LocationName,
                importLocationItemDto.LocationDescription,
                importLocationItemDto.LocationLatitude,
                importLocationItemDto.LocationLongitude,
                importLocationItemDto.LocationAddress,
                importLocationItemDto.LocationType,
                importLocationItemDto.LocationAccuracy,
                importLocationItemDto.LocationSource
            );
            _context.Locations.Add(location);
            await _context.SaveChangesAsync(cancellationToken); // Save to get Location.Id

            // Create FamilyLocation entity
            var newFamilyLocation = new FamilyLocation(request.FamilyId, location.Id);
            _context.FamilyLocations.Add(newFamilyLocation);
            importedFamilyLocations.Add(newFamilyLocation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load the Location for each FamilyLocation to map to DTO
        foreach (var fl in importedFamilyLocations)
        {
            await ((DbContext)_context).Entry(fl).Reference(fl => fl.Location).LoadAsync(cancellationToken);
        }

        var importedLocationDtos = _mapper.Map<List<FamilyLocationDto>>(importedFamilyLocations);

        return Result<List<FamilyLocationDto>>.Success(importedLocationDtos);
    }
}
