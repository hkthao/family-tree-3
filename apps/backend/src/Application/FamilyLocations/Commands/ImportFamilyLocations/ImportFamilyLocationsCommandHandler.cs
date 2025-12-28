using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Linq;

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

        var importedLocations = new List<FamilyLocation>();

        foreach (var importLocationItemDto in request.Locations)
        {
            // Kiểm tra xem vị trí đã tồn tại theo tên và FamilyId chưa
            var existingLocation = await _context.FamilyLocations
                .FirstOrDefaultAsync(fl => fl.FamilyId == request.FamilyId && fl.Name == importLocationItemDto.Name, cancellationToken);

            if (existingLocation != null)
            {
                _logger.LogInformation("Vị trí gia đình với tên '{LocationName}' đã tồn tại cho FamilyId {FamilyId}. Bỏ qua nhập.", importLocationItemDto.Name, request.FamilyId);
                continue;
            }

            var newLocation = _mapper.Map<FamilyLocation>(importLocationItemDto);
            newLocation.FamilyId = request.FamilyId; // Gán FamilyId từ request
            _context.FamilyLocations.Add(newLocation);
            importedLocations.Add(newLocation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var importedLocationDtos = _mapper.Map<List<FamilyLocationDto>>(importedLocations);

        return Result<List<FamilyLocationDto>>.Success(importedLocationDtos);
    }
}
