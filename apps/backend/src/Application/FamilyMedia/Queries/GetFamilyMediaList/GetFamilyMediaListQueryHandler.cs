using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;
using backend.Application.FamilyMedia.Queries.Specifications; // For ApplyOrdering

namespace backend.Application.FamilyMedia.Queries.GetFamilyMediaList;

public class GetFamilyMediaListQueryHandler : IRequestHandler<GetFamilyMediaListQuery, Result<PaginatedList<FamilyMediaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public GetFamilyMediaListQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<FamilyMediaDto>>> Handle(GetFamilyMediaListQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanViewFamily(request.FamilyId))
        {
            return Result<PaginatedList<FamilyMediaDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var specification = new FamilyMediaListSpecification(request.FamilyId, request.Filters);

        var query = _context.FamilyMedia
            .WithSpecification(specification)
            .AsNoTracking();

        // Apply ordering
        query = query.ApplyOrdering(request.OrderBy);

        var paginatedList = await PaginatedList<FamilyMediaDto>.CreateAsync(
            query.ProjectTo<FamilyMediaDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<FamilyMediaDto>>.Success(paginatedList);
    }
}
