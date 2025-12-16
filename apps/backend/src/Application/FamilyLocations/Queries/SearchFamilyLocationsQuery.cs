using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyLocations.Queries;

public record SearchFamilyLocationsQuery : PaginatedQuery, IRequest<Result<PaginatedList<FamilyLocationListDto>>>
{
    public Guid? FamilyId { get; init; }
    public string? SearchQuery { get; init; }
}

public class SearchFamilyLocationsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchFamilyLocationsQuery, Result<PaginatedList<FamilyLocationListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<FamilyLocationListDto>>> Handle(SearchFamilyLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FamilyLocations
            .AsNoTracking()
            .AsQueryable();

        query = query.WithSpecification(new FamilyLocationByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new FamilyLocationSearchTermSpecification(request.SearchQuery));
        query = query.WithSpecification(new FamilyLocationOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedList = await query
            .ProjectTo<FamilyLocationListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<FamilyLocationListDto>>.Success(paginatedList);
    }
}
