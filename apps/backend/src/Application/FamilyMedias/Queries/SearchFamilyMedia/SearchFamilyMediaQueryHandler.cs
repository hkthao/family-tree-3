using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.FamilyMedias.Queries.Specifications;

namespace backend.Application.FamilyMedias.Queries.SearchFamilyMedia;

public class SearchFamilyMediaQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMapper mapper) : IRequestHandler<SearchFamilyMediaQuery, Result<PaginatedList<FamilyMediaDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<FamilyMediaDto>>> Handle(SearchFamilyMediaQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanViewFamily(request.FamilyId))
        {
            return Result<PaginatedList<FamilyMediaDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var specification = new SearchFamilyMediaSpecification(request.FamilyId, request);

        var query = _context.FamilyMedia
            .WithSpecification(specification)
            .AsNoTracking();

        // Apply ordering
        query = query.ApplyOrdering(request.SortOrder);

        var paginatedList = await PaginatedList<FamilyMediaDto>.CreateAsync(
            query.ProjectTo<FamilyMediaDto>(_mapper.ConfigurationProvider),
            request.Page,
            request.ItemsPerPage
        );

        return Result<PaginatedList<FamilyMediaDto>>.Success(paginatedList);
    }
}
