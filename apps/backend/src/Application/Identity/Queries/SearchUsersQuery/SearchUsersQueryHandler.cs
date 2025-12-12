using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Users.Specifications;

namespace backend.Application.Identity.Queries;

public class SearchUsersQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<UserDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        query = query.WithSpecification(new UserSearchTermSpecification(request.SearchQuery));

        var paginatedList = await query
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<UserDto>>.Success(paginatedList);
    }
}
