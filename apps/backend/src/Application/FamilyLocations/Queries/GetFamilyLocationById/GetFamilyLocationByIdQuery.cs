using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyLocations.Queries.GetFamilyLocationById;

public record GetFamilyLocationByIdQuery(Guid Id) : IRequest<Result<FamilyLocationDto>>;

public class GetFamilyLocationByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetFamilyLocationByIdQuery, Result<FamilyLocationDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<FamilyLocationDto>> Handle(GetFamilyLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.FamilyLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<FamilyLocationDto>.NotFound($"FamilyLocation with id {request.Id} not found.");
        }

        return Result<FamilyLocationDto>.Success(_mapper.Map<FamilyLocationDto>(entity));
    }
}
