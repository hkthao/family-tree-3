using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyLocations.Queries.GetFamilyLocationByAddress;

public class GetFamilyLocationByAddressQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetFamilyLocationByAddressQuery, Result<FamilyLocationDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<FamilyLocationDto>> Handle(GetFamilyLocationByAddressQuery request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .Where(l => l.Address == request.Address)
            .FirstOrDefaultAsync(cancellationToken);

        if (location == null)
        {
            return Result<FamilyLocationDto>.NotFound("Không tìm thấy địa điểm với địa chỉ đã cung cấp.");
        }

        var familyLocation = await _context.FamilyLocations
            .Where(fl => fl.LocationId == location.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (familyLocation == null)
        {
            // This case might mean a location exists but is not associated with any family,
            // or the specific family location was not found.
            return Result<FamilyLocationDto>.NotFound("Không tìm thấy địa điểm gia đình nào liên quan đến địa chỉ này.");
        }

        var familyLocationDto = _mapper.Map<FamilyLocationDto>(familyLocation);
        return Result<FamilyLocationDto>.Success(familyLocationDto);
    }
}